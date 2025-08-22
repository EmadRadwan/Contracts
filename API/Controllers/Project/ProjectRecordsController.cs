using API.Controllers.OData;
using Application.Manufacturing;
using Application.Projects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace API.Controllers.Project;

public class ProjectRecordsController : BaseODataController<WorkEffortRecord>
{
    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> Get(ODataQueryOptions<WorkEffortRecord> options)
    {
        var query = await Mediator.Send(new ProjectsList.Query { Options = options });
        return await HandleODataQueryAsync(query, options);
    }
}