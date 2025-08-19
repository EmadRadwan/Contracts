using API.Controllers.OData;
using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class CostComponentRecordsController : BaseODataController<CostComponentRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<CostComponentRecord> options)
    {
        var query = await Mediator.Send(new ListCostComponents.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}