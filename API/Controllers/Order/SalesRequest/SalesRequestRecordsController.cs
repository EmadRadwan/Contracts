using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using API.Controllers.OData;
using Application.Order.SalesRequests;

namespace API.Controllers.Order.SalesRequest;

public class SalesRequestRecordsController : BaseODataController2<SalesRequestRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<SalesRequestRecord> options)
    {
        var language = GetLanguage();

        var query = await Mediator.Send(new ListSalesRequestsQuery.Query
        {
            Options = options,
            Language = language
        });

        return await HandleODataQueryAsync(query, options);
    }
}