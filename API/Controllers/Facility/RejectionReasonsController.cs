using Application.Facilities.RejectionReasons;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Facility;

public class RejectionReasonsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}