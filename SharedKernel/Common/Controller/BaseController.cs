using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Controller;

public abstract class BaseController : ControllerBase
{
    private readonly IMediator _mediator;

    protected BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// Gửi request thông qua Mediator và trả về Ok(result)
    /// </summary>
    protected async Task<IActionResult> Send<TResponse>(IRequest<TResponse> request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    /// <summary>
    /// Nếu bạn muốn dùng Push cho Create/Update
    /// </summary>
    protected async Task<IActionResult> Push<TResponse>(IRequest<TResponse> request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}
