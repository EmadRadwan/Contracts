using API.Controllers.OData;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Accounting.OrganizationGlSetting
{
    public class OrganizationGlBulkEditRecordsController : BaseODataController<OrganizationGlAccountRecord>
    {
        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get([FromQuery] string companyId, ODataQueryOptions<OrganizationGlAccountRecord> options)
        {
            var language = GetLanguage();
            var query = await Mediator.Send(new ListOrganizationChartOfAccounts.Query 
            { 
                Options = options, 
                CompanyId = companyId,
                Language = language
            });
            return await HandleODataQueryAsync(query, options);
        }
    }
}
