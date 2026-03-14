using API.Controllers.OData;
using Application.CRM.Leads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.CRM;

public class LeadRecordsController : BaseODataController<LeadRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<LeadRecord> options)
    {
        var query = await Mediator.Send(new ListLeads.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}
