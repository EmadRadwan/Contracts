using API.Controllers.OData;
using Application.Facilities.InventoryTransfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Facility;

public class InventoryTransferRecordsController : BaseODataController<InventoryTransferRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<InventoryTransferRecord> options)
    {
        var query = await Mediator.Send(new ListInventoryTransfers.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}