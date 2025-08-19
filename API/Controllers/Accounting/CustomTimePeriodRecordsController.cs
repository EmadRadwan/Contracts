using API.Controllers.OData;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class CustomTimePeriodRecordsController : BaseODataController<CustomTimePeriodRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<CustomTimePeriodRecord> options)
    {
        var query = await Mediator.Send(new ListCustomTimePeriods.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}