using API.Controllers.OData;
using Application.Facilities.FacilityInventories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Facility;

public class FacilityInventoryItemRecordsController : BaseODataController<FacilityInventoryItemRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<FacilityInventoryItemRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListFacilityInventoriesByInventoryItem.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}