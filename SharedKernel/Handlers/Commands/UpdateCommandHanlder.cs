using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Commands;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Wrapper.Exceptions;
using SharedKernel.Common.Wrapper.Response;
using SharedKernel.Handlers.BaseHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Handlers.Commands;

public abstract class UpdateCommandHanlder<TDbContext, TEntity, TKey> : BaseCommandHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class where TKey : struct
{
    public UpdateCommandHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseDto<TDto>> Handle<TDto>(UpdateCommand<TDto,TKey> request, CancellationToken cancellationToken) where TDto : class
    {
        try
        {
            DbSet<TEntity> _repo = _context.Set<TEntity>();
            var entity = await _context.Set<TEntity>().FindAsync(new object[] { request.id }, cancellationToken);
            if (entity is null)
                throw new AppException(HttpStatusCode.NotFound, ResponseMessage.NotFound);

            MapToEntity(request.data, entity);
            await ValidateAsync(request.data, entity, cancellationToken);
            _repo.Update(entity);
            if (await _context.SaveChangesAsync(cancellationToken) >= 1)
            {
                return new ResponseDto<TDto>
                {
                    data = _mapper.Map<TEntity, TDto>(entity),
                    message = ResponseMessage.UpdateSuccess,
                    success = true,
                    status_code = (int)HttpStatusCode.OK
                };
            }

            throw new AppException(HttpStatusCode.BadRequest, ResponseMessage.UpdateFailed);
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected virtual void MapToEntity<TDto>(TDto dto, TEntity entity) where TDto : class
    {
        _mapper.Map(dto, entity);
    }
    protected virtual Task ValidateAsync<TDto>(TDto dto, TEntity entity, CancellationToken cancellationToken) where TDto : class
    {
        return Task.CompletedTask;
    }
}