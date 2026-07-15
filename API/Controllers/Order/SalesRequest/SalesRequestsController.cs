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

    [HttpPost("{id}/reset")]
    public async Task<IActionResult> Reset(string id)
    {
        var command = new ResetSalesRequest.Command { SalesRequestId = id };
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
    
    // Controllers/SalesRequestsController.cs

    [HttpGet("{id}/installments")]
    public async Task<ActionResult<List<SalesRequestInstallmentDto>>> GetInstallments(string id)
    {
        var installments = await Mediator.Send(new GetInstallmentsQuery.Query { SalesRequestId = id });
        return Ok(installments);
    }

    [HttpGet("{id}/chequeable-payments")]
    public async Task<IActionResult> GetChequeablePayments(string id)
    {
        var payments = await Mediator.Send(new ListChequeableSalesRequestPayments.Query { SalesRequestId = id });
        return Ok(payments);
    }

    [HttpPost("{id}/cheques")]
    public async Task<IActionResult> RecordCheques(string id, [FromBody] RecordSalesRequestCheques.Command command)
    {
        command.SalesRequestId = id;
        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet("by-date-range")]
    public async Task<ActionResult<List<SalesRequestRecord>>> GetSalesRequestsByDateRange(
        [FromQuery] string fromDate,
        [FromQuery] string toDate,
        CancellationToken ct = default)
    {
        var language = GetLanguage();
        var query = new ListSalesRequestsByDateRange.Query
        {
            FromDate = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null),
            ToDate = DateTime.ParseExact(toDate, "yyyy-MM-dd", null).AddDays(1).AddTicks(-1), // end of day
            Language = language
        };

        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("approvedLov")]
    public async Task<IActionResult> GetApprovedSalesRequestsLov(
        [FromQuery] string? searchTerm,
        [FromQuery] int skip = 0,
        [FromQuery] int pageSize = 20)
    {
        return HandleResult(await Mediator.Send(new GetApprovedSalesRequestsLov.Query
        {
            SearchTerm = searchTerm,
            Skip = skip,
            PageSize = pageSize
        }));
    }
}