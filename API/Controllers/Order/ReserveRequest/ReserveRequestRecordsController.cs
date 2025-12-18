using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using API.Controllers.OData;
using Application.Order.SalesRequests;

namespace API.Controllers.Order.SalesRequest;

public class ReserveRequestRecordsController : BaseODataController2<ListReserveRequestsQuery.ReserveRequestRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ListReserveRequestsQuery.ReserveRequestRecord> options)
    {
        var language = GetLanguage();

        var query = await Mediator.Send(new ListReserveRequestsQuery.Query
        {
            Options = options,
            Language = language
        });

        return await HandleODataQueryAsync(query, options);
    }
}