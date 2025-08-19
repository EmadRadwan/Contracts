using Application.Catalog.ProductPromos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Catalog;

public class ProductPromosController : BaseApiController
{
    [HttpGet("getProductPromos", Name = "GetProductPromos")]
    public async Task<ActionResult> GetProductPromos()
    {
        return HandleResult(await Mediator.Send(new GetProductPromos.Query()));
    }
}