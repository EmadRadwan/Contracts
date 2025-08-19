/*using API.Controllers.OData;
using Application.Order.JobOrders;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Order;

public class JobOrderRecordsController : BaseODataController<OrderRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<OrderRecord> options)
    {
        var query = await Mediator.Send(new ListJobOrders.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}*/