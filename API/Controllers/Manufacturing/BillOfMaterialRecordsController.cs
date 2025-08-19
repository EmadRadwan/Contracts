using API.Controllers.OData;
using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Manufacturing;

public class BillOfMaterialRecordsController : BaseODataController<BillOfMaterialRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<BillOfMaterialRecord> options)
    {
        var query = await Mediator.Send(new ListBOMProductsQuery.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}