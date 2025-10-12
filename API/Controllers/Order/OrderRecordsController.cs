using API.Controllers.OData;
using Application.Order.Orders;
using Application.Order.Orders.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Order;

public class OrderRecordsController : BaseODataController<OrderRecord>
{
    // REFACTOR: Add orderType parameter to Get method to filter by order type
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<OrderRecord> options, [FromQuery] string orderType = null)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListOrders.Query { Options = options, Language = language, OrderType = orderType });
        return await HandleODataQueryAsync(query, options);
    }
}