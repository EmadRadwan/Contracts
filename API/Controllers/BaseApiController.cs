using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    private IMediator _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
        .GetService<IMediator>();

    protected string GetLanguage()
    {
        // Retrieve the 'Accept-Language' header
        return Request.Headers["Accept-Language"].ToString();
    }

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        Console.WriteLine(result);
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);
        if (result.IsSuccess && result.Value == null)
            return NotFound();
        return BadRequest(new ProblemDetails { Title = result.Error });
    } 
    
    protected ActionResult HandleResults<T>(Results<T> result)
    {
        Console.WriteLine(result);
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Value != null)
            return Ok(result.Value);
        if (result.IsSuccess && result.Value == null)
            return NotFound();
        return BadRequest(new
        {
            title = result.ErrorMessage,
            status = 400,
            errorCode = result.ErrorCode // REFACTOR: Explicitly include errorCode in response
        });
    }

    protected ActionResult HandlePagedResult<T>(Result<PagedList<T>> result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Value != null)
        {
            Response.AddPaginationHeader(result.Value.MetaData);
            return Ok(result.Value);
        }

        if (result.IsSuccess && result.Value == null)
            return NotFound();
        return BadRequest(result.Error);
    }
}