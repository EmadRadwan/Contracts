using Application.Catalog.ProductPrices;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductPricesController : BaseApiController
{
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductPrices(string productId)
    {
        return HandleResult(await Mediator.Send(new ListProductPrices.Query { ProductId = productId, Language = GetLanguage() }));
    }

    [HttpPut("updateProductPrice")]
    public async Task<IActionResult> UpdateProductPrice(ProductPrice productPrice)
    {
        return HandleResult(await Mediator.Send(new UpdateProductPrice.Command { ProductPrice = productPrice }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductPrice(ProductPrice productPrice)
    {
        return HandleResult(await Mediator.Send(new CreateProductPrice.Command { ProductPrice = productPrice }));
    }
}