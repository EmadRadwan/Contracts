using Application.Accounting.PaymentGroup;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentGroupTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetPaymentGroupTypes()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetPaymentGroupTypes.Query { Language = language }));
    }
}