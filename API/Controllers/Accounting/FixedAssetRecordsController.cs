using API.Controllers.OData;
using Application.Shipments.FixedAssets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class FixedAssetRecordsController : BaseODataController<FixedAssetRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<FixedAssetRecord> options)
    {
        var query = await Mediator.Send(new ListFixedAssets.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}