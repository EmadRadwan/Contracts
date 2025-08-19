using Application.Facilities.FacilityLocations;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class FacilityLocationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFacilityLocationsLov()
    {
        return HandleResult(await Mediator.Send(new GetFacilityLocationsLov.Query()));
    }
}