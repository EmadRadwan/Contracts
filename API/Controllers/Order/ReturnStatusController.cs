using Application.Order.Orders;
using Application.Order.Orders.Returns;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class ReturnStatusController : BaseApiController
{
    [HttpGet("listReturnItemsStatus")]
    public async Task<ActionResult<List<StatusItemDto>>> GetReturnStatusItems([FromQuery] string? returnId, [FromQuery] string returnHeaderType)
    {
        // REFACTOR: Validate returnHeaderType to prevent null or invalid values
        if (string.IsNullOrEmpty(returnHeaderType) || (returnHeaderType != "V" && returnHeaderType != "C"))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid returnHeaderType",
                Detail = "returnHeaderType must be 'V' or 'C'",
                Status = 400
            });
        }

        var query = new GetReturnStatusItemsQuery { ReturnId = returnId ?? "", ReturnHeaderType = returnHeaderType };
        var result = await Mediator.Send(query);
        return Ok(result);
    }
    
}