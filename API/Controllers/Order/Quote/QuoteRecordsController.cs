using API.Controllers.OData;
using Application.Order.Quotes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Order.Quote;

public class QuoteRecordsController : BaseODataController<QuoteRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<QuoteRecord> options)
    {
        var query = await Mediator.Send(new ListQuotes.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}