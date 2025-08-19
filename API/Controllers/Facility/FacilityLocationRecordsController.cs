using API.Controllers.OData;
using Application.Facilities.FacilityLocations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Facility;

public class FacilityLocationRecordsController : BaseODataController<FacilityLocationRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<FacilityLocationRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListFacilityLocations.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}