using Application.Order.JobOrders;
using Application.Order.Orders;
using Application.Order.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class JobOrdersController : BaseApiController
{
    [HttpGet("{partyId}/getSalesOrdersByPartyId")]
    public async Task<IActionResult> GetSalesOrdersByPartyId(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListSalesOrdersByPartyId.Query { PartyId = partyId }));
    }


    [HttpGet("{orderId}/getJobOrderItems")]
    public async Task<IActionResult> GetJobOrderItems(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListJobOrderItems.Query { OrderId = orderId }));
    }


    [HttpGet("{orderId}/getJobOrderAdjustments")]
    public async Task<IActionResult> GetJobOrderAdjustments(string orderId)
    {
        return HandleResult(await Mediator.Send(new ListJobOrderAdjustments.Query { OrderId = orderId }));
    }


    [HttpPost("createJobOrderFromQuote", Name = "CreateJobOrderFromQuote")]
    public async Task<IActionResult> CreateJobOrderFromQuote(QuoteDto quoteDto)
    {
        return HandleResult(await Mediator.Send(new CreateJobOrderFromQuote.Command { QuoteDto = quoteDto }));
    }
}