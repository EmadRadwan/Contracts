using Application.Catalog.ProductPrices;
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

    [HttpGet("getSimplePurchaseProductsLov", Name = "GetSimplePurchaseProductsLov")]
    public async Task<IActionResult> GetSimplePurchaseProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(
            await Mediator.Send(new GetSimplePurchaseProductsLov.Query { Params = param, Language = language }));
    }

    [HttpGet("getSimpleProductsLov", Name = "GetSimpleProductsLov")]
    public async Task<IActionResult> GetSimpleProductsLov([FromQuery] ProductLovParams param)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new GetSimpleProductsLov.Query { Params = param, Language = language }));
    }

    [HttpGet("getSimpleApartmentsLov", Name = "GetSimpleApartmentsLov")]
    public async Task<IActionResult> GetSimpleApartmentsLov([FromQuery] ApartmentLovParams param)
    {
        // REFACTOR: Route to the new MediatR handler
        return HandleResult(await Mediator.Send(new GetSimpleApartmentsLov.Query { Params = param }));
    }

    [HttpGet("{projectId}/getSimpleApartmentsByProjectLov", Name = "GetSimpleApartmentsByProjectLov")]
    public async Task<IActionResult> GetSimpleApartmentsByProjectLov(string projectId, [FromQuery] ApartmentLovParams param)
    {
        // REFACTOR: Route to the new MediatR handler
        return HandleResult(await Mediator.Send(new GetSimpleApartmentsByProjectLov.Query { ProjectId = projectId, Params = param }));
    }

    [HttpGet("getRawMaterialProductsLov")]
    public async Task<IActionResult> GetRawMaterialProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetRawMaterialProductsLov.Query { Params = param }));
    }

    [HttpGet("getServiceProductsLov")]
    public async Task<IActionResult> GetServiceProductsLov([FromQuery] ProductLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetServiceProductsLov.Query { Params = param }));
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


    [HttpPut("updateProduct")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProduct.Command command)
    {
        // REFACTOR: No change needed – Mediator already receives the wrapped command
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPatch("{productId}/reserve")]
    public async Task<IActionResult> ReserveApartment(string productId)
    {
        return HandleResult(await Mediator.Send(new ReserveApartment.Command { ProductId = productId }));
    }

    [HttpPost("createProduct", Name = "CreateProduct")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProduct.Command command)
    {
        return HandleResult(await Mediator.Send(command));
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

    // Presentation/API/Controllers/ProductController.cs
    [HttpGet("lastUnitPrice")]
    public async Task<IActionResult> GetLastUnitPrice(
        [FromQuery] string productId,
        [FromQuery] string facilityId)
    {
        if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(facilityId))
            return BadRequest("productId and facilityId are required");

        var result = await Mediator.Send(new GetLastUnitPrice.Query
        {
            ProductId = productId,
            FacilityId = facilityId
        });

        return HandleResult(result);
    }
}