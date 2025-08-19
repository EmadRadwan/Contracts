using Application.Parties.ContactMechTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Party;

public class ContactMechTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}