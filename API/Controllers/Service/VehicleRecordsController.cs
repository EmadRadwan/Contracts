using API.Controllers.OData;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Service;

public class VehicleRecordsController : BaseODataController<VehicleRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<VehicleRecord> options)
    {
        var query = await Mediator.Send(new ListVehicles.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}