using Application.Shipments.InvoiceItemTypeDtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class InvoiceItemTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}