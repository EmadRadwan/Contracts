using Application.Catalog.ProductFeatures;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Catalog;

public class ProductFeaturesController : BaseApiController
{
    [HttpGet("colors")]
    public async Task<IActionResult> GetProductFeatureColors()
    {
        return HandleResult(await Mediator.Send(new ListProductFeatureColors.Query { Language = GetLanguage() }));
    }
    
    [HttpGet("sizes")]
    public async Task<IActionResult> GetProductFeatureSizes()
    {
        return HandleResult(await Mediator.Send(new ListProductFeatureSizes.Query { Language = GetLanguage() }));
    } 
    
    [HttpGet("trademarks")]
    public async Task<IActionResult> GetProductFeatureTrademarks()
    {
        return HandleResult(await Mediator.Send(new ListProductFeatureTrademarks.Query { Language = GetLanguage() }));
    }
}