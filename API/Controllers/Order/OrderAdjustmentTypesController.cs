using Application.Order.OrderAdjustmentTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class OrderAdjustmentTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListQuoteAdjustmentTypes()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListOrderAdjustmentTypes.Query{Language = language}));
    }
}