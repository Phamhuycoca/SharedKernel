using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Handlers.BaseHandler;

public class BaseQueryHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class
{
    protected readonly DbContext _context;

    protected readonly IMapper _mapper;

    protected readonly DbSet<TEntity> _repo;

    protected readonly IMediator _mediator;

    public BaseQueryHanlder(TDbContext context, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
        _repo = context.Set<TEntity>();
    }
}
