using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class CreditCardTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCreditCardTypes()
    {
        return HandleResult(await Mediator.Send(new GetCreditCardTypes.Query()));
    }
}