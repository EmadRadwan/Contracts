using Application.Catalog.Products;
using Application.Manufacturing;
using Application.Projects;
using Application.WorkEfforts;
using Microsoft.AspNetCore.Mvc;
using CostComponentCalcDto = Application.Manufacturing.CostComponentCalcDto;

namespace API.Controllers.WorkEffort;

public class ProjectController : BaseApiController
{
    [HttpPost("createProject", Name = "CreateProject")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] ProjectDto project)
    {
        return HandleResult(await Mediator.Send(new CreateProject.Command { ProjectDto = project }));
    }

    [HttpPut("updateProject", Name = "UpdateProject")]
    public async Task<ActionResult<ProjectDto>> UpdateProject([FromBody] ProjectDto project)
    {
        if (string.IsNullOrEmpty(project.WorkEffortId))
        {
            return BadRequest("Project ID is required.");
        }

        return HandleResult(await Mediator.Send(new UpdateProject.Command { ProjectDto = project }));
    }
}