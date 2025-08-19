using API.Controllers.OData;
using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class BOMProductComponentRecordsController : BaseODataController<BOMProductComponentRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<BOMProductComponentRecord> options)
    {
        var query = await Mediator.Send(new ListBOMProductComponentsQuery.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}