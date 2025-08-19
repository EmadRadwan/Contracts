using Application.PartyTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PartyTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}