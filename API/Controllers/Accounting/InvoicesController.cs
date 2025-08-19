using Application.Shipments.Invoices;
using Application.Accounting.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class InvoicesController : BaseApiController
{
    [HttpGet("{invoiceId}/getInvoiceItems")]
    public async Task<IActionResult> GetInvoiceItems(string invoiceId)
    {
        return HandleResult(await Mediator.Send(new ListInvoiceItems.Query
            { InvoiceId = invoiceId, Language = GetLanguage() }));
    }


    [HttpPost("createInvoice", Name = "CreateInvoice")]
    public async Task<IActionResult> CreateInvoice(InvoiceDto3 invoiceDto)
    {
        return HandleResult(await Mediator.Send(new CreateInvoice.Command { InvoiceDto = invoiceDto }));
    }

    [HttpPut("updateInvoice", Name = "UpdateInvoice")]
    public async Task<IActionResult> UpdateInvoice(InvoiceDto3 invoiceDto)
    {
        return HandleResult(await Mediator.Send(new UpdateInvoice.Command { InvoiceDto = invoiceDto }));
    }

    [HttpGet("{paymentId}/listNotAppliedInvoices")]
    public async Task<IActionResult> ListNotAppliedInvoices(string paymentId)
    {
        return HandleResult(await Mediator.Send(new ListNotAppliedInvoices.Query { PaymentId = paymentId }));
    }

    [HttpGet("getStatusItems")]
    public async Task<IActionResult> GetStatusItems()
    {
        return HandleResult(await Mediator.Send(new GetInvoiceStatusItems.Query()));
    }

    [HttpPost("CalculateInvoiceTotal")]
    public async Task<ActionResult<InvoiceTotalsDto>> CalculateTotal([FromBody] InvoiceIdDto request)
    {
        var result = await Mediator.Send(new CalculateInvoiceTotal.Query { InvoiceId = request.InvoiceId });
        return Ok(result);
    }

    [HttpPost("ChangeInvoiceStatus")]
    public async Task<ActionResult<InvoiceStatusDto>> ChangeInvoiceStatus([FromBody] InvoiceStatusDto request)
    {
        var result = await Mediator.Send(new ChangeInvoiceStatus.Query
        {
            InvoiceId = request.InvoiceId,
            ActualCurrency = request.ActualCurrency,
            PaidDate = request.PaidDate,
            StatusDate = request.StatusDate,
            StatusId = request.StatusId
        });
        return Ok(result);
    }

    [HttpPost("CreateInvoiceItem")]
    public async Task<IActionResult> CreateInvoiceItem([FromBody] InvoiceItemParameters parameters)
    {
        var command = new CreateInvoiceItem.Command
        {
            Parameters = parameters
        };

        var result = await Mediator.Send(command);

        if (result.IsError)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("{invoiceId}")]
    public async Task<IActionResult> GetInvoiceById(string invoiceId)
    {
        var language = GetLanguage();

        return HandleResult(
            await Mediator.Send(new GetInvoiceById.Query { InvoiceId = invoiceId, Language = language }));
    }
    
    [HttpGet("getInvoiceItemTypesByInvoiceId")]
    public async Task<IActionResult> GetInvoiceItemTypesByInvoiceId(
        [FromQuery] string invoiceId,
        CancellationToken cancellationToken)
    {
        // REFACTOR: Updated validation to check invoiceId
        // Purpose: Ensures invoiceId is provided as a required parameter
        // Improvement: Simplifies validation, aligns with minimalistic design
        if (string.IsNullOrEmpty(invoiceId))
        {
            return BadRequest("invoiceId is required.");
        }

        // REFACTOR: Updated query to use GetInvoiceItemTypesByInvoiceId
        // Purpose: Matches the new handler that queries by invoiceId
        // Improvement: Maintains consistency with handler changes
        var query = new GetInvoiceItemTypesByInvoiceId.Query
        {
            InvoiceId = invoiceId,
            Language = GetLanguage()
        };

        var result = await Mediator.Send(query, cancellationToken);

        // REFACTOR: Reused response handling from original controller
        // Purpose: Maintains consistent success and error response patterns
        // Improvement: Ensures predictable API behavior
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return StatusCode(500, new { error = result.Error });
    }

}