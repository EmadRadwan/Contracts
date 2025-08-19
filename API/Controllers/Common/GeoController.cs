using Application.Geos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class GeoCountryController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListCountry()
    {
        return HandleResult(await Mediator.Send(new ListCountry.Query()));
    }
}