using API.Controllers.OData;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class InternalAccountingOrganizationRecordsController : BaseODataController<InternalAccountingOrganizationRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<InternalAccountingOrganizationRecord> options)
    {
        var query = await Mediator.Send(new ListInternalAccountingOrganizations.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}