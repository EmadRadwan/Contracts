using Application.Shipments.InvoiceTypes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class InvoiceTypesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return HandleResult(await Mediator.Send(new List.Query()));
    }
}