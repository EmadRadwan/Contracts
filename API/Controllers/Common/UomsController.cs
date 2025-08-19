using Application.Common.Uoms;
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
}