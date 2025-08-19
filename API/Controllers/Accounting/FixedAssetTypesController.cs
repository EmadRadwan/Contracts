using Application.Shipments.FixedAssets;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class FixedAssetTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}