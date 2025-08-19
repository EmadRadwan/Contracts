using Application.Shipments.FixedAssets;
using Application.Shipments.PaymentMethodTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class FixedAssetStdCostTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new ListFixedAssetStdCostTypes.Query()));
    }
}