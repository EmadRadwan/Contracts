using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class TermTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListTermTypesLov()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListTermTypesLov.Query {Language = language}));
    }
}