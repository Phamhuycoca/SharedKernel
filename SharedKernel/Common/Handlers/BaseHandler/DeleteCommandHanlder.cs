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

public abstract class DeleteCommandHanlder<TDbContext, TEntity, TKey> : BaseCommandHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class where TKey : IEquatable<TKey>
{
    public DeleteCommandHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseDto<bool>> Handle(DeleteCommand<TKey> request, CancellationToken cancellationToken)
    {

        var entity = await _context.Set<TEntity>()
         .FindAsync(new object[] { request.id }, cancellationToken);
        if (entity is null)
            throw new AppException(HttpStatusCode.NotFound, ResponseMessage.NotFound);

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return new ResponseDto<bool>
        {
            data = true,
            message = ResponseMessage.DeleteSuccess,
            success = true,
            status_code = (int)HttpStatusCode.OK
        };

    }
}
