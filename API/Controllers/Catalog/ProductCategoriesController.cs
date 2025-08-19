using Application.Catalog.ProductCategories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductCategoriesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }

    [HttpGet("getHierarchicalCategories")]
    public async Task<IActionResult> GetHierarchicalCategories()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListHierarchicalCategories.Query {Language = language}));
    }
    
    [HttpGet("getHierarchicalCategoriesRawMaterials")]
    public async Task<IActionResult> GetHierarchicalCategoriesRawMaterials()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListHierarchicalCategoriesRawMaterials.Query {Language = language}));
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetProductCategory(string productId)
    {
        return HandleResult(await Mediator.Send(new ListProductCategories.Query { ProductId = productId }));
    }

    [HttpPut("updateProductCategoryMember")]
    public async Task<IActionResult> UpdateProductCategory(ProductCategoryMember productCategoryMember)
    {
        return HandleResult(await Mediator.Send(new UpdateProductCategory.Command
            { ProductCategoryMember = productCategoryMember }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductCategory(ProductCategoryMember productCategoryMember)
    {
        return HandleResult(await Mediator.Send(new CreateProductCategory.Command
            { ProductCategoryMember = productCategoryMember }));
    }
}