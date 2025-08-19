using Application.Catalog.ProductAssociationTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductAssociationTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        // REFACTOR: Pass language from query parameter, defaulting to English to match example
        return HandleResult(await Mediator.Send(new ListProductAssociationTypes.Query { Language = GetLanguage() }));
    }
}