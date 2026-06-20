using API.Controllers.OData;
using Application.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Project;

public class ProjectCommissionRateRecordsController : BaseODataController<ProjectCommissionRateRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<ProjectCommissionRateRecord> options)
    {
        var query = await Mediator.Send(new ListProjectCommissionRates.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}
