using API.Controllers.OData;
using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class ProductionRunRecordsController : BaseODataController<WorkEffortRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<WorkEffortRecord> options)
    {
        var query = await Mediator.Send(new ListProductionRuns.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}