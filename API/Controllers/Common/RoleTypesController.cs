using Application.RoleTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class RoleTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}