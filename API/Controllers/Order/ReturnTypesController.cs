using Application.Order.Orders.Returns.ReturnTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class ReturnTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}