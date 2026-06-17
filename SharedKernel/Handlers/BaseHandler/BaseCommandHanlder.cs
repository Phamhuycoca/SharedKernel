using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Handlers.BaseHandler;

public class BaseCommandHanlder<TDbContext, TEntity> where TDbContext : DbContext where TEntity : class
{
    protected readonly DbContext _context;

    protected readonly IMapper _mapper;

    protected readonly DbSet<TEntity> _repo;

    protected readonly IMediator _mediator;

    public BaseCommandHanlder(TDbContext context, IMapper mapper, IMediator mediator)
    {
        _context = context;
        _mapper = mapper;
        _mediator = mediator;
        _repo = context.Set<TEntity>();
    }
}