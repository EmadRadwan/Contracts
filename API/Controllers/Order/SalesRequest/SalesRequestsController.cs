using Application.Order.SalesRequests;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order.SalesRequest;

public class SalesRequestsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesRequest.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSalesRequest.Command command)
    {
        // The client already puts the id inside the DTO → enforce it matches the route
        if (command.SalesRequestDto?.SalesRequestId != id)
            return BadRequest("SalesRequestId in payload must match the route id.");

        return HandleResult(await Mediator.Send(command));
    }
    
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id)
    {
        // Simple validation – ensures route id is used
        var command = new ApproveSalesRequest.Command { SalesRequestId = id };
        return HandleResult(await Mediator.Send(command));
    }
    
    [HttpPost("calculate-meter-price")]
    public async Task<IActionResult> CalculateMeterPrice([FromBody] CalculateMeterPrice.Query query)
    {
        decimal newMeterPrice = await Mediator.Send(query);

        return Ok(newMeterPrice);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteSalesRequest.Command { SalesRequestId = id };
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return HandleResult(result); // Will return appropriate error (e.g., NotFound, BadRequest)

        // 204 No Content – standard response for successful DELETE with no body
        return NoContent();
    }

}