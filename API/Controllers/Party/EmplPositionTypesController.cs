using Application.Parties.Parties;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Party;

public class EmplPositionTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new ListEmplPositionTypes.Query()));
    }
}