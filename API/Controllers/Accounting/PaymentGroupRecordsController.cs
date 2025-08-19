using API.Controllers.OData;
using Application.Accounting.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class PaymentGroupRecordsController : BaseODataController<PaymentGroupRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<PaymentGroupRecord> options)
    {
        var language = GetLanguage();
        var query = await Mediator.Send(new ListPaymentGroups.Query { Options = options, Language = language });
        return await HandleODataQueryAsync(query, options);
    }
}