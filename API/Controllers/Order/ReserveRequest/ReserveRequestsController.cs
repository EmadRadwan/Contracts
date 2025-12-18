using Application.Order.ReserveRequests;
using Application.Order.SalesRequests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order.ReserveRequest;

public class ReserveRequestsController : BaseApiController
{
    [HttpPost("reserve")]
    public async Task<IActionResult> CreateReserveRequest([FromBody] CreateReserveRequest.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }
    
    [HttpPut()]
    public async Task<IActionResult> EditReserveRequest([FromBody] EditReserveRequest.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }

}