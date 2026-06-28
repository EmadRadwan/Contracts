using API.Controllers.OData;
using Application.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Project;

public class SalesCommissionRecordsController : BaseODataController<SalesCommissionRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(
        ODataQueryOptions<SalesCommissionRecord> options,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var query = await Mediator.Send(new ListSalesCommissions.Query
        {
            Options = options,
            FromDate = fromDate,
            ToDate = toDate,
        });
        return await HandleODataQueryAsync(query, options);
    }
}
