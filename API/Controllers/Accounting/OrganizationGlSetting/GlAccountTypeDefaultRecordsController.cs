using API.Controllers.OData;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class GlAccountTypeDefaultRecordsController : BaseODataController<GlAccountTypeDefaultRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get([FromQuery] string companyId,
        ODataQueryOptions<GlAccountTypeDefaultRecord> options)
    {
        var query = await Mediator.Send(
            new ListGlAccountTypeDefaults.Query { Options = options, CompanyId = companyId });
        return await HandleODataQueryAsync(query, options);
    }
}