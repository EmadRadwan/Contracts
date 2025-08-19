using Application.CustomerRequestType;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CustomerRequestTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}