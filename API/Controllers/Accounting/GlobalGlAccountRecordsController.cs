using API.Controllers.OData;
using Application.Shipments.GlobalGlSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting;

public class GlobalGlAccountRecordsController : BaseODataController<GlAccountRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<GlAccountRecord> options)
    {
        var language = GetLanguage(); // Access the language here if needed

        var query = await Mediator.Send(new ListGlobalChartOfAccounts.Query
        {
            Options = options,
            Language = language
        });
        return await HandleODataQueryAsync(query, options);
    }
}