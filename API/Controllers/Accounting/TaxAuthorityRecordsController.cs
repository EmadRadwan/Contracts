using API.Controllers.OData;
using Application.Shipments.Taxes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class TaxAuthorityRecordsController : BaseODataController<TaxAuthorityRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<TaxAuthorityRecord> options)
    {
        var query = await Mediator.Send(new ListTaxAuthorities.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}