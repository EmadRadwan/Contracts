using API.Controllers.OData;
using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class CostComponentCalcRecordsController : BaseODataController<CostComponentCalcRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<CostComponentCalcRecord> options)
    {
        var query = await Mediator.Send(new ListCostComponentCalcs.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}