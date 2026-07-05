using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Controller;

public abstract class BaseController : BaseApiController
{
    /// <summary>
    /// Send request thông qua MediatR và trả về HTTP 200.
    /// </summary>
    protected async Task<IActionResult> Send<TResponse>(
        IRequest<TResponse> request)
    {
        try
        {
            var result = await Mediator.Send(request);
            return Ok(result);
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }

    /// <summary>
    /// Send request thông qua MediatR và trả về HTTP 201.
    /// </summary>
    protected async Task<IActionResult> SendCreated<TResponse>(
        IRequest<TResponse> request)
    {
        try
        {
            var result = await Mediator.Send(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }

    /// <summary>
    /// Send request không có dữ liệu trả về.
    /// </summary>
    protected async Task<IActionResult> Send(
        IRequest request)
    {
        try
        {
            await Mediator.Send(request);
            return NoContent();
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }
}
