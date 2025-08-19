using Application.Shipments.PaymentTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetPaymentTypes()
    {
        var language = GetLanguage();
        return HandleResult(await
            Mediator.Send(new GetPaymentTypes.Query
                { Language = language }
            ));
    }
}