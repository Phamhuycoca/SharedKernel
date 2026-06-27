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

namespace SharedKernel.Handlers.Queries;

public abstract class GetByIdQueryHanlder<TDbContext, TEntity, TKey> : BaseQueryHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class where TKey : struct
{
    public GetByIdQueryHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseDto<TDto>> Handle<TDto>(GetByIdQueryCommand<TKey> request, CancellationToken cancellationToken) where TDto : class
    {

        var entity = await _context.Set<TEntity>()
        .FindAsync(new object[] { request.id }, cancellationToken);

        if (entity is null)
            throw new AppException(HttpStatusCode.NotFound, ResponseMessage.NotFound);

        return new ResponseDto<TDto>
        {
            data = _mapper.Map<TDto>(entity),
            success = true,
            message = ResponseMessage.GetSuccess
        };

    }
}