using API.Controllers.OData;
using Application.Shipments.ForeignExchangeRates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class ForeignExchangeRateRecordsController : BaseODataController<UomConversionDatedRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<UomConversionDatedRecord> options)
    {
        var query = await Mediator.Send(new ListForeignExchangeRates.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}