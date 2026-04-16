using API.Controllers.OData;
using Application.Accounting.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class PaymentRecordsWithDueStatusController : BaseODataController2<PaymentRecord>
{
    // REFACTOR: Updated Get method to accept paymentType query parameter
    [HttpGet]
    public async Task<IActionResult> Get(ODataQueryOptions<PaymentRecord> options, [FromQuery] string? paymentType = null)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListPaymentsWithDueStatus.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }

    [HttpGet("by-date-range")]
    public async Task<ActionResult<ListPaymentsWithDueStatusByDateRange.ListPaymentsWithDueStatusResponse>> GetByDateRange(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        CancellationToken ct = default)
    {
        var language = GetLanguage();

        var query = new ListPaymentsWithDueStatusByDateRange.Query
        {
            FromDate = fromDate,
            ToDate = toDate,
            Language = language
        };

        var result = await Mediator.Send(query, ct);
        return Ok(result);
    }
}