namespace API.Controllers.Order;
using Microsoft.AspNetCore.Mvc;
using Application.Order.Orders.Returns;
public class ReturnsController : BaseApiController
{
    [HttpPost("create", Name = "CreateReturnHeader")]
    public async Task<IActionResult> CreateReturnHeader(ReturnDto returnDto)
    {
        return HandleResult(await Mediator.Send(new CreateReturnHeader.Command { ReturnDto = returnDto }));
    }
    
    [HttpGet("{returnId}/listReturnItems/{orderId?}", Name = "ListReturnItems")]
    public async Task<IActionResult> ListReturnItems(string returnId, string orderId = null)
    {
        // Send the query to the mediator
        return HandleResult(await Mediator.Send(new ListReturnItems.Query
        {
            ReturnId = returnId,
            OrderId = orderId // This can be null
        }));
    }
    
    [HttpGet("{returnId}/getPartyOrders", Name = "GetPartyOrders")]
    public async Task<IActionResult> GetPartyOrders(string returnId)
    {
        return HandleResult(await Mediator.Send(new GetPartyOrders.Query
        {
            ReturnId = returnId
        }));
    }
    
    [HttpGet("{orderId}/getOrderSummary", Name = "GetOrderSummary")]
    public async Task<IActionResult> GetOrderSummary(string orderId)
    {
        return HandleResult(await Mediator.Send(new GetOrderSummary.Query
        {
            OrderId = orderId
        }));
    }
    
    [HttpGet("{orderId}/getReturnableItems", Name = "GetReturnableItems")]
    public async Task<ActionResult<ReturnableItemsResult>> GetReturnableItems(string orderId)
    {
        return await Mediator.Send(new GetReturnableItems.Query
        {
            OrderId = orderId
        });
    }

    [HttpPost("processReturnItemsOrAdjustments", Name = "ProcessReturnItemsOrAdjustments")]
    public async Task<ActionResult<Result<List<ReturnItemOrAdjustmentResult>>>> ProcessReturnItemsOrAdjustments(
        [FromBody] List<ReturnItemOrAdjustmentContext> contexts)
    {
        var result = await Mediator.Send(new ProcessReturnItemsOrAdjustments.Command
        {
            Contexts = contexts
        });

        // Wrap the result in an ActionResult
        return Ok(result);
    }

    [HttpGet("{returnId}")]
    public async Task<IActionResult> GetReturnById(string returnId)
    {
        if (string.IsNullOrEmpty(returnId))
        {
            return BadRequest("Return ID is required");
        }

        var result = await Mediator.Send(new GetReturnById { ReturnId = returnId });
        if (result == null)
        {
            return NotFound($"Return with ID {returnId} not found");
        }

        return Ok(result);
    }

}