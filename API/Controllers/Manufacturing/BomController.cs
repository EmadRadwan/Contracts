using Application.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Manufacturing;

public class BomController : BaseApiController
{
    [HttpGet("{productId}/{quantityToProduce}/{currencyUomId}/getSimulatedBomCost")]
    public async Task<IActionResult> GetSimulatedBomCost(string productId, decimal quantityToProduce, string currencyUomId)
    {
        return HandleResult(await Mediator.Send(new GetSimulatedBomCost.Query{ ProductId = productId, QuantityToProduce = quantityToProduce, CurrencyUomId = currencyUomId }));
    }
}