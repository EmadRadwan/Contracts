using Application.Shipments.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class FixedAssetStdCostsController : BaseApiController
{
    [HttpGet("{fixedAssetId}/listFixedAssetStdCosts")]
    public async Task<IActionResult> List(string fixedAssetId)
    {
        return HandleResult(await Mediator.Send(new ListFixedAssetStdCosts.Query { FixedAssetId = fixedAssetId }));
    }
}