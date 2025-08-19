using Application.Order.Orders.Returns.ReturnReasons;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class ReturnReasonsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}