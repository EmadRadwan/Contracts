using Application.Accounting.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class FinAccountStatusController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetFinAccountStaus()
    {
        return HandleResult(await Mediator.Send(new GetFinAccountStatus.Query()));
    }
}