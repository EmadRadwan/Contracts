using Application.Catalog.ProductAssociations;
using Application.Catalog.ProductAssocsiations;
using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductAssociationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string productId)
    {
        // REFACTOR: Pass productId and language from query parameters, defaulting language to English
        return HandleResult(await Mediator.Send(new ListProductAssociations.Query 
        { 
            ProductId = productId, 
            Language = GetLanguage() 
        }));
    }
    
    [HttpGet("listProductsToAssociateTo", Name = "ListProductsToAssociateTo")]
    public async Task<IActionResult> ListProductsToAssociateTo(
        [FromQuery] string excludeProductId)
    {
        return HandleResult(await Mediator.Send(new ListProductsToAssociateTo.Query
        {
            ExcludeProductId = excludeProductId,
        }));
    }
    
    [HttpPost("createProductAssociation")]
    public async Task<IActionResult> CreateProductAssociation(CreateProductAssociation.ProductAssociationDto? productAssociation)
    {
        return HandleResult(await Mediator.Send(new CreateProductAssociation.Command { ProductAssociationDto = productAssociation }));
    }
    
    // Controller endpoint
    [HttpPut("updateProductAssociation")]
    public async Task<IActionResult> UpdateProductAssociation(UpdateProductAssociation.ProductAssociationDto? productAssociation)
    {
        return HandleResult(await Mediator.Send(new UpdateProductAssociation.Command { ProductAssociationDto = productAssociation }));
    }
}