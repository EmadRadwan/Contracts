using Application.Catalog.ProductStores;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Catalog;

public class ProductStoresController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> ListProductStoreFacilities()
    {
        return HandleResult(await Mediator.Send(new ListProductStoreFacilities.Query()));
    }

    [HttpGet("GetProductStorePaymentMethods")]
    public async Task<ActionResult> GetProductStorePaymentMethods()
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListProductStorePaymentMethods.Query {Language = language} ));
    }
    
    [HttpGet("GetProductStorePaymentSettings")]
    public async Task<ActionResult> GetProductStorePaymentSettings()
    {
        return HandleResult(await Mediator.Send(new ListProductStorePaymentSettings.Query()));
    }

    [HttpGet("GetProductStoreForLoggedInUser")]
    public async Task<ActionResult> GetProductStoreForLoggedInUser()
    {
        return HandleResult(await Mediator.Send(new GetProductStoreForLoggedInUser.Query()));
    }
}