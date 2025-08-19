using Application.Order.QuoteAdjustmentTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class QuoteAdjustmentTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListQuoteAdjustmentTypes()
    {
        return HandleResult(await Mediator.Send(new ListQuoteAdjustmentTypes.Query()));
    }
}