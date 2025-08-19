using Application.Catalog.ProductTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListProductTypes.Query {Language = language}));
    }
}