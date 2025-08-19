using Application.UomTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UomTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}