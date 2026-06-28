using Microsoft.AspNetCore.Mvc;
using SharedKernel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Controller;

[Route("api/[controller]")]
public class BaseCrudApiController<
    TKey,
    TGetAllQuery,
    TGetByIdQuery,
    TCreateCommand,
    TUpdateCommand,
    TDeleteCommand> : BaseApiController
    where TKey : struct
    where TGetAllQuery : GetAllQueryCommand
    where TGetByIdQuery : GetByIdQueryCommand<TKey>
    where TCreateCommand : class
    where TUpdateCommand : class
    where TDeleteCommand : DeleteCommand<TKey>
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> GetAll(TGetAllQuery query)
    {
        return Ok(await base.Mediator.Send((object)query, default(CancellationToken)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(TGetByIdQuery query)
    {
        try
        {
            return Ok(await base.Mediator.Send((object)query, default(CancellationToken)));
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create(TCreateCommand request)
    {
        try
        {
            return Created("", await base.Mediator.Send((object)request, default(CancellationToken)));
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(TUpdateCommand request)
    {
        try
        {
            return Ok(await base.Mediator.Send((object)request, default(CancellationToken)));
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> Delete(TDeleteCommand request)
    {
        try
        {
            await base.Mediator.Send((object)request, default(CancellationToken));
            return NoContent();
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            return StatusCode(Convert.ToInt32(HttpStatusCode.BadRequest), ex.Message);
        }
    }
}