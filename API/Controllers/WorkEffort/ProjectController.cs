using Application.Catalog.Products;
using Application.Manufacturing;
using Application.Parties.Parties;
using Application.Project;
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
    
    [HttpPost("createProjectCertificate", Name = "CreateProjectCertificate")]
    public async Task<ActionResult<ProjectCertificateRecord>> CreateProjectCertificate([FromBody] ProjectCertificateRecord certificate)
    {
        var result = await Mediator.Send(new CreateProjectCertificate.Command { Certificate = certificate });
        return HandleResult(result);
    }
    
    [HttpGet("{workEffortId}/getCertificateItems")]
    public async Task<IActionResult> GetCertificateItems(string workEffortId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListCertificateItems.Query { WorkEffortId = workEffortId, Language = language }));
    }
    
    [HttpGet("getProjectsLov", Name = "GetProjectsLov")]
    public async Task<IActionResult> GetProjectsLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetProjectsLov.Query { Params = param }));
    }
}