using Application.Catalog.ProductFacilities;
using Application.ProductFacilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductFacilitiesController : BaseApiController
{
    [HttpGet("{productId}")]
    public async Task<IActionResult> ListProductFacilities(string productId)
    {
        return HandleResult(await Mediator.Send(new ListProductFacilities.Query { ProductId = productId }));
    }

    [HttpPut("updateProductFacility")]
    public async Task<IActionResult> UpdateProductFacility(ProductFacilityDto productFacility)
    {
        return HandleResult(
            await Mediator.Send(new UpdateProductFacility.Command { ProductFacility = productFacility }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductFacility(ProductFacilityDto productFacility)
    {
        return HandleResult(
            await Mediator.Send(new CreateProductFacility.Command { ProductFacility = productFacility }));
    }
}