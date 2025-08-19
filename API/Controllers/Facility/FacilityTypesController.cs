using Application.Facilities.Facilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class FacilityTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListFacilityTypes()
    {
        return HandleResult(await Mediator.Send(new ListFacilityTypes.Query()));
    }
}