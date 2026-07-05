using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Constants;
using SharedKernel.Common.Handlers.Commands;
using SharedKernel.Common.Wrappers.ErrorResponse;
using SharedKernel.Common.Wrappers.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Handlers.BaseHandler;

public abstract class CreateCommandHanlder<TDbContext, TEntity> : BaseCommandHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class
{
    public CreateCommandHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseDto<TDto>> Handle<TDto>(CreateCommand<TDto> request, CancellationToken cancellationToken) where TDto : class
    {

        TEntity entity = MapToEntity(request.data);
        await ValidateAsync(request.data, entity, cancellationToken);
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        if (await _context.SaveChangesAsync(cancellationToken) >= 1)
        {
            return new ResponseDto<TDto>
            {
                data = _mapper.Map<TEntity, TDto>(entity),
                message = ResponseMessage.CreateSuccess,
                success = true,
                status_code = (int)HttpStatusCode.OK
            };
        }

        throw new AppException(HttpStatusCode.BadRequest, ResponseMessage.CreateFailed);

    }

    protected virtual TEntity MapToEntity<TDto>(TDto dto) where TDto : class
    {
        return _mapper.Map<TDto, TEntity>(dto);
    }
    protected virtual Task ValidateAsync<TDto>(TDto dto, TEntity entity, CancellationToken cancellationToken) where TDto : class
    {
        return Task.CompletedTask;
    }
}
