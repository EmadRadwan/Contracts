using Application.Shipments.PaymentMethodTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentMethodTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }

    [HttpGet("getPaymentMethodTypes")]
    public async Task<IActionResult> GetPaymentMethodTypes()
    {
        return HandleResult(await Mediator.Send(new GetPaymentMethodTypes.Query()));
    }
}