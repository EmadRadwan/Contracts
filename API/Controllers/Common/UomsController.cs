using Application.Catalog.Products;
using Application.Common.Uoms;
using Application.Common.UOMs;
using Application.Uoms;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UomsController : BaseApiController
{
    [HttpGet("currency")]
    public async Task<IActionResult> ListCurrency()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListCurrency.Query{Language = language}));
    }

    [HttpGet("quantity")]
    public async Task<IActionResult> ListQuantity()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListQuantity.Query {Language = language}));
    }
    
    [HttpGet("conversionDated")]
    public async Task<IActionResult> ListConversionDated()
    {
        return HandleResult(await Mediator.Send(new ListConversionDated.Query()));
    }
    
    [HttpGet("getUOMsLov", Name = "GetUOMsLov")]
    public async Task<IActionResult> GetUOMsLov([FromQuery] ProductLovParams param)
    {
        // REFACTOR: Extract language from Accept-Language header
        // Purpose: Pass language preference to the handler for multilingual support
        // Context: Retrieves Accept-Language header set by axios client
        var language = Request.Headers["Accept-Language"].ToString() ?? "en";

        return HandleResult(await Mediator.Send(new GetUOMsLov.Query { Params = param, Language = language }));
    }
}