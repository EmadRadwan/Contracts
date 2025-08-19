using Application.ProductPriceTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductPriceTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query {Language = GetLanguage()}));
    }
}