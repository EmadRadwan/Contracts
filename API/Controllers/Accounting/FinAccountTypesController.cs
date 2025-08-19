using Application.Accounting.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class FinAccountTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFinAccountTypes()
    {
        return HandleResult(await Mediator.Send(new GetFinAccountTypes.Query()));
    }
}