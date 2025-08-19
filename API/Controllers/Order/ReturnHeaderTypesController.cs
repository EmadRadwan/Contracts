using Application.Order.Orders.Returns.ReturnHeaderMethodTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Order;

public class ReturnHeaderTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}