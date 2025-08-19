using Application.Order.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Quote;

public class QuotesController : BaseApiController
{
    [HttpGet("{quoteId}/getQuoteItems")]
    public async Task<IActionResult> GetQuoteItems(string quoteId)
    {
        return HandleResult(await Mediator.Send(new ListQuoteItems.Query { QuoteId = quoteId }));
    }

    [HttpGet("{quoteId}/getQuoteAdjustments")]
    public async Task<IActionResult> GetQuoteAdjustments(string quoteId)
    {
        return HandleResult(await Mediator.Send(new ListQuoteAdjustments.Query { QuoteId = quoteId }));
    }

    [HttpGet("{quoteItemId}/getQuoteItemProduct")]
    public async Task<IActionResult> GetQuoteItemProduct(string quoteItemId)
    {
        return HandleResult(await Mediator.Send(new GetQuoteItemProduct.Query { QuoteItemId = quoteItemId }));
    }


    [HttpPost("createQuote", Name = "CreateQuote")]
    public async Task<IActionResult> CreateQuote(QuoteDto quoteDto)
    {
        return HandleResult(await Mediator.Send(new CreateQuote.Command { QuoteDto = quoteDto }));
    }

    [HttpPut("updateOrApproveQuote", Name = "UpdateOrApproveQuote")]
    public async Task<IActionResult> UpdateOrApproveQuote(QuoteDto quoteDto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrApproveQuote.Command { QuoteDto = quoteDto }));
    }
}