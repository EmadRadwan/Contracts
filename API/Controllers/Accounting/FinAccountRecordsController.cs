using API.Controllers.OData;
using Application.Shipments.FinAccounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class FinAccountRecordsController : BaseODataController<FinAccountRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<FinAccountRecord> options)
    {
        var query = await Mediator.Send(new ListFinAccounts.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}