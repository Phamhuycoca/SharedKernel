using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Common.Handlers.Commands;
using SharedKernel.Common.Wrappers.Response;
using SharedKernel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Handlers.BaseHandler;

public abstract class GetAllQueryHanlder<TDbContext, TEntity> : BaseQueryHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class
{
    public GetAllQueryHanlder(TDbContext context, IMapper mapper, IMediator mediator)
        : base(context, mapper, mediator)
    {
    }

    public async Task<ResponseListDto<TDto>> Handle<TDto>(GetAllQueryCommand request, CancellationToken cancellationToken) where TDto : class
    {
        Dictionary<string, object> sortObject = request.sort.ConvertSortObj();
        dynamic filterObj = request.filter.ConvertFilterObj();
        IQueryable<TEntity> query4 = _context.Set<TEntity>().AsQueryable();
        query4 = QueryBuilder(query4, filterObj, request.search, request);
        query4 = SortBuilder(query4, sortObject);
        int total = await query4.CountAsync(cancellationToken);
        query4 = PagingBuilder(query4, request.page, request.page_size);
        return new ResponseListDto<TDto>(_data: await query4.ProjectTo(_mapper.ConfigurationProvider, Array.Empty<Expression<Func<TDto, object>>>()).ToListAsync(cancellationToken), _meta: new Meta(request.page, request.page_size, total));
    }

    protected virtual IQueryable<TEntity> QueryBuilder(IQueryable<TEntity> query, dynamic filter, string search, GetAllQueryCommand request)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> SortBuilder(IQueryable<TEntity> query, Dictionary<string, dynamic> sortObject)
    {
        string text = "";
        foreach (string key in sortObject.Keys)
        {
            if (sortObject.ContainsKey(key))
            {
                string text2 = ((sortObject[key] == 1) ? "ascending" : "descending");
                text = text + key + " " + text2 + ",";
            }
        }

        text = text.Substring(0, text.Length - 1);
        query = query.OrderBy(text);
        return query;
    }

    protected virtual IQueryable<TEntity> PagingBuilder(IQueryable<TEntity> query, int page, int page_size)
    {
        if (page > 0 && page_size > 0)
        {
            int count = (page - 1) * page_size;
            query = query.Skip(count).Take(page_size);
        }

        return query;
    }
}
