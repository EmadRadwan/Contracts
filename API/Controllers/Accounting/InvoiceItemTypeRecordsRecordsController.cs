using API.Controllers.OData;
using Application.Shipments.InvoiceItemTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class InvoiceItemTypeRecordsController : BaseODataController<InvoiceItemTypeRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<InvoiceItemTypeRecord> options)
    {
        var query = await Mediator.Send(new ListInvoiceItemTypes.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}