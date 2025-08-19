using Application.Catalog.ProductSuppliers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductSuppliersController : BaseApiController
{
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductSuppliers(string productId)
    {
        return HandleResult(await Mediator.Send(new ListProductSuppliers.Query { ProductId = productId }));
    }

    [HttpPut("updateProductSupplier")]
    public async Task<IActionResult> UpdateProductSupplier(UpdateProductSupplier.SupplierProductUpdateDto supplierProduct)
    {
        return HandleResult(
            await Mediator.Send(new UpdateProductSupplier.Command { SupplierProduct = supplierProduct }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductSupplier(CreateProductSupplier.SupplierProductCreateDto supplierProduct)
    {
        return HandleResult(
            await Mediator.Send(new CreateProductSupplier.Command { SupplierProduct = supplierProduct }));
    }
}