using API.Controllers.OData;
using Application.Manufacturing;
using Application.WorkEfforts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class ProductionRunReservationRecordsController : BaseODataController<WorkEffortReservationRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<WorkEffortReservationRecord> options)
    {
        var query = await Mediator.Send(new ListWorkEffortsWithReservations.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}