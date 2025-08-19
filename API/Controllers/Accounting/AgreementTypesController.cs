using Application.Shipments.Agreement;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class AgreementTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListAgreementTypes()
    {
        return HandleResult(await Mediator.Send(new ListAgreementTypesLov.Query()));
    }
}