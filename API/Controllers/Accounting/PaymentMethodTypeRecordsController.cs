using API.Controllers.OData;
using Application.Shipments.PaymentMethodTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class PaymentMethodTypeRecordsController : BaseODataController<PaymentMethodTypeRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<PaymentMethodTypeRecord> options)
    {
        var query = await Mediator.Send(new ListPaymentMethodTypes.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}