using API.Controllers.OData;
using Application.Accounting.Invoices;
using Application.Shipments.Invoices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class InvoiceRecordsController : BaseODataController2<InvoiceView>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<InvoiceView> options)
    {
        var language = GetLanguage(); // Assumes GetLanguage() retrieves language from headers, query params, or user settings
        var query = await Mediator.Send(new ListInvoices.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}