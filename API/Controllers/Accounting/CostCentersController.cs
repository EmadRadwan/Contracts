using Application.CostCenters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class CostCentersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCostCenters([FromQuery] string? type = null)
    {
        var language = GetLanguage(); // نفس الطريقة اللي بتستخدمها في PaymentTypes

        return HandleResult(await Mediator.Send(new GetCostCenters.Query
        {
            Language = language,
            Type = type?.ToLower() // "in" | "out" | null
        }));
    }
}