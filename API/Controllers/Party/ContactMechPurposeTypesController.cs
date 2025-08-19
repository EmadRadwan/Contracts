using Application.ContactMechPurposeTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ContactMechPurposeTypes : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}