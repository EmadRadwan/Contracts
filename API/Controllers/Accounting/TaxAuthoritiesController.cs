using Application.Shipments.Taxes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class TaxAuthoritiesController : BaseApiController
{
    [HttpGet("getTaxAuthorities")]
    public async Task<IActionResult> GetTaxAuthorities()
    {
        return HandleResult(await Mediator.Send(new GetTaxAuthorities.Query()));
    }
}