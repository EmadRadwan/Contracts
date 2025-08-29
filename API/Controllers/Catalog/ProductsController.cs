using Application.Catalog.ProductPromos;
using Application.Catalog.Products;
using Application.Order.Orders;
using Application.Order.Quotes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Catalog;

public class ProductsController : BaseApiController
{
    [HttpGet("getSalesProductsLov", Name = "GetSalesProductsLov")]
    public async Task<IActionResult> GetSalesProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetSalesProductsLov.Query { Params = param, Language = language }));
    }

    [HttpGet("getFinishedProductsLov", Name = "GetFinishedProductsLov")]
    public async Task<IActionResult> GetFinishedProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(
            await Mediator.Send(new GetFinishedProductsLov.Query { Params = param, Language = language }));
    }

    [HttpGet("getProductPrice/{productId}", Name = "GetProductPrice")]
    public async Task<IActionResult> GetProductPrice(string productId)
    {
        // REFACTOR: Simplified endpoint to pass productId directly to the query, aligning with the single-product focus of the handler.
        return HandleResult(await Mediator.Send(new GetProductPriceById.Query { ProductId = productId }));
    }

    [HttpGet("getProductDetails/{productId}", Name = "GetProductDetails")]
    public async Task<IActionResult> GetProductDetails(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProductDetailsById.Query
            { ProductId = productId, Language = GetLanguage() }));
    }

    [HttpGet("getPhysicalInventoryProductsLov", Name = "GetPhysicalInventoryProductsLov")]
    public async Task<IActionResult> GetPhysicalInventoryProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetPhysicalInventoryProductsLov.Query
            { Params = param, Language = language }));
    }


    [HttpGet("getPurchaseProductsLov", Name = "GetPurchaseProductsLov")]
    public async Task<IActionResult> GetPurchaseProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(
            await Mediator.Send(new GetPurchaseProductsLov.Query { Params = param, Language = language }));
    }
    
    [HttpGet("getSimpleProductsLov", Name = "GetSimpleProductsLov")]
    public async Task<IActionResult> GetSimpleProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetSimpleProductsLov.Query { Params = param }));
    }

    [HttpGet("getInventoryItemProductsLov", Name = "GetInventoryItemProductsLov")]
    public async Task<IActionResult> GetInventoryItemProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetInventoryItemProductsLov.Query { Params = param }));
    }

    [HttpGet("getFacilityProductsLov", Name = "GetFacilityProductsLov")]
    public async Task<IActionResult> GetFacilityProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetFacilityProductsLov.Query { Params = param }));
    }

    [HttpGet("getAssocsProductsLov", Name = "GetAssocsProductsLov")]
    public async Task<IActionResult> GetAssocsProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetAssocsProductsLov.Query { Params = param }));
    }

    [HttpGet("getProductsLov", Name = "GetProductsLov")]
    public async Task<IActionResult> GetProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetProductsLov.Query { Params = param }));
    }

    [HttpGet("getFacilityInventoryItemProduct", Name = "GetFacilityInventoryItemProduct")]
    public async Task<IActionResult> GetFacilityInventoryItemProduct([FromQuery] FacilityInventoryItemParams param)
    {
        return HandleResult(await Mediator.Send(new GetFacilityInventoryItemProduct.Query { Params = param }));
    }

    [HttpGet("{productId}/getInventoryItemColors", Name = "GetInventoryItemColors")]
    public async Task<IActionResult> GetInventoryItemColors([FromRoute] string productId)
    {
        return HandleResult(await Mediator.Send(new GetInventoryItemColors.Query { ProductId = productId }));
    }

    [HttpGet("getServicesLov", Name = "GetServicesLov")]
    public async Task<IActionResult> GetServicesLov([FromQuery] ServiceLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetServicesLov.Query { Params = param }));
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProduct(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProduct.Query { ProductId = productId }));
    }

    [HttpGet("{productId}/getFinishedProductsForWIP")]
    public async Task<IActionResult> GetFinishedProductsForWIP(string productId)
    {
        return HandleResult(await Mediator.Send(new GetFinishedProductsForWIP.Query { ProductId = productId }));
    }

    [HttpGet("getFinishedProductsLov2", Name = "GetFinishedProductsLov2")]
    public async Task<IActionResult> GetFinishedProductsLov2([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetFinishedProductsLov2.Query { Params = param }));
    }

    [HttpGet("{productId}/getAvailableProductPromotions")]
    public async Task<IActionResult> GetAvailableProductPromotions(string productId)
    {
        return HandleResult(await Mediator.Send(new GetAvailableProductPromotions.Query { ProductId = productId }));
    }

    [HttpPost("applyOrderItemPromo", Name = "ApplyOrderItemPromo")]
    public async Task<IActionResult> ApplyOrderItemPromo(OrderItemDto2 orderItemDto2)
    {
        return HandleResult(await Mediator.Send(new ApplyOrderItemPromo.Command
            { OrderItemDto2 = orderItemDto2 }));
    }

    [HttpPost("applyQuoteItemPromo", Name = "ApplyQuoteItemPromo")]
    public async Task<IActionResult> ApplyQuoteItemPromo(QuoteItemDto2 quoteItemDto2)
    {
        return HandleResult(await Mediator.Send(new ApplyQuoteItemPromo.Command
            { QuoteItemDto2 = quoteItemDto2 }));
    }

    [HttpPost("calculateQuoteItemPromoProductDiscount", Name = "CalculateQuoteItemPromoProductDiscount")]
    public async Task<IActionResult> CalculateQuoteItemPromoProductDiscount(QuoteItemDto2 quoteItemDto2)
    {
        return HandleResult(await Mediator.Send(new CalculateQuoteItemPromoProductDiscount.Command
            { QuoteItemDto2 = quoteItemDto2 }));
    }


    [HttpPut("updateProduct", Name = "UpdateProduct")]
    public async Task<IActionResult> UpdateProduct(ProductDto product)
    {
        return HandleResult(await Mediator.Send(new UpdateProduct.Command { ProductDto = product }));
    }

    [HttpPost("createProduct", Name = "CreateProduct")]
    public async Task<IActionResult> CreateProduct(ProductDto product)
    {
        return HandleResult(await Mediator.Send(new CreateProduct.Command { ProductDto = product }));
    }

    [HttpGet("{productId}/calculateProductCosts")]
    public async Task<IActionResult> CalculateProductCosts(string productId)
    {
        return HandleResult(await Mediator.Send(new CalculateProductCosts.Query { ProductId = productId }));
    }

    [HttpGet("{productId}/getProductQuantityUom")]
    public async Task<IActionResult> GetProductQuantityUom(string productId)
    {
        return HandleResult(await Mediator.Send(new GetProductQuantityUom.Query { ProductId = productId }));
    }
}