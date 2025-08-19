using API.Controllers.OData;
using Application.Shipments.BillingAccounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class BillingAccountRecordsController : BaseODataController<BillingAccountRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<BillingAccountRecord> options)
    {
        var query = await Mediator.Send(new ListBillingAccounts.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}