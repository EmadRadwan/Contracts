using API.Controllers.OData;
using Application.Facilities.FacilityInventories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Facility;

public class FacilityInventoryRecordsController : BaseODataController<FacilityInventoryRecordView>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<FacilityInventoryRecordView> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListFacilityInventoriesByProduct2.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}