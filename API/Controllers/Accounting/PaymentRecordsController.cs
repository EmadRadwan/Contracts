using API.Controllers.OData;
using Application.Accounting.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class PaymentRecordsController : BaseODataController2<PaymentRecord>
{
    [HttpGet]
    public async Task<IActionResult> Get(
        ODataQueryOptions<PaymentRecord> options,
        [FromQuery] string? paymentType = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListPayments.Query
        {
            Options = options,
            Language = language,
            PaymentType = paymentType,
            FromDate = fromDate,
            ToDate = toDate,
        });
        return await HandleODataQueryAsync(query, options);
    }
}