using API.Controllers.OData;
using Application.Order.Orders.Returns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Order.Return;

public class ReturnRecordsController : BaseODataController<ReturnRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ReturnRecord> options)
    {
        var query = await Mediator.Send(new ListReturns.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}