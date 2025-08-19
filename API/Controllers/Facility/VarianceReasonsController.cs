using Application.Facilities.Facilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class VarianceReasonsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListVarianceReasons()
    {
        return HandleResult(await Mediator.Send(new ListVarianceReasons.Query()));
    }
}