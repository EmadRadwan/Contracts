using API.Controllers.OData;
using Application.Accounting.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class PaymentRecordsWithDueStatusController : BaseODataController<PaymentRecord>
{
    // REFACTOR: Updated Get method to accept paymentType query parameter
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<PaymentRecord> options, [FromQuery] string? paymentType = null)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListPaymentsWithDueStatus.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}