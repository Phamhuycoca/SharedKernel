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

public class DeleteCommandHanlder<TDbContext, TEntity,TKey> : BaseCommandHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class where TKey : struct
{
    public DeleteCommandHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseDto<bool>> Handle(DeleteCommand<TKey> request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Set<TEntity>()
             .FindAsync(new object[] { request.id }, cancellationToken);
            if (entity is null)
                throw new AppException(HttpStatusCode.NotFound, ResponseMessage.NotFound);

            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();
            return new ResponseDto<bool>
            {
                data = true,
                message = ResponseMessage.DeleteSuccess,
                success = true,
                status_code = (int)HttpStatusCode.OK
            };
        }
        catch (Exception ex)
        {
            throw new AppException(HttpStatusCode.BadRequest, ex.Message);
        }
    }
}