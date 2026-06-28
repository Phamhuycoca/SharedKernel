using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Controller;

[ApiController]
public class BaseApiController : ControllerBase
{
    private ISender _mediator = null;

    protected ISender Mediator => _mediator ?? (_mediator = base.HttpContext.RequestServices.GetRequiredService<ISender>());
}
