using Application.Parties.Parties;
using Application.Projects;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<ProjectCertificateDto>> CreateProjectCertificate([FromBody] ProjectCertificateDto certificate)
    {
        var result = await Mediator.Send(new CreateProjectCertificate.Command { Certificate = certificate });
        return HandleResult(result);
    }
    
    [HttpPut("updateProjectCertificate/{workEffortId}", Name = "UpdateProjectCertificate")]
    public async Task<ActionResult<ProjectCertificateDto>> UpdateProjectCertificate(string workEffortId, [FromBody] ProjectCertificateDto certificate)
    {
        certificate.WorkEffortId = workEffortId;
        var result = await Mediator.Send(new UpdateProjectCertificate.Command { Certificate = certificate });
        return HandleResult(result);
    }
    
    [HttpGet("{workEffortId}/getCertificateItems")]
    public async Task<IActionResult> GetCertificateItems(string workEffortId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListCertificateItems.Query { WorkEffortId = workEffortId, Language = language }));
    }
    
    [HttpGet("byParty")]
    public async Task<IActionResult> GetCertificatesByParty(
        [FromQuery] string? contractorId,
        [FromQuery] string? supplierId,
        [FromQuery] string certificateType)
    {
        var language = GetLanguage();

        var result = await Mediator.Send(new GetCertificatesByParty.Query
        {
            ContractorId = contractorId,
            SupplierId = supplierId,
            Language = language
        });
        return HandleResult(result);
    }
    
    [HttpGet("getProjectsLov", Name = "GetProjectsLov")]
    public async Task<IActionResult> GetProjectsLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetProjectsLov.Query { Params = param }));
    }
    
    [HttpGet("subProjects/{projectId}")]
    public async Task<IActionResult> GetSubProjects(string projectId)
    {
        var language = GetLanguage();
        return HandleResult(await Mediator.Send(new ListSubProjects.Query { ProjectId = projectId, Language = language }));
    }
    
    [HttpGet("{workEffortId}/items")]
    public async Task<IActionResult> GetMultiPaymentItems(string workEffortId)
    {
        return HandleResult(await Mediator.Send(new ListMultiPaymentItems.Query { WorkEffortId = workEffortId }));
    }
    
    [HttpPost("multiPaymentCertificate")]
    public async Task<ActionResult<MultiPaymentCertificateDto>> CreateMultiPaymentCertificate([FromBody] MultiPaymentCertificateDto certificate)
    {
        var result = await Mediator.Send(new CreateMultiPaymentCertificate.Command { Certificate = certificate });
        return HandleResult(result);
    }
    
    [HttpPut("multiPaymentCertificate/{workEffortId}")]
    public async Task<ActionResult<MultiPaymentCertificateDto>> UpdateMultiPaymentCertificate(
        string workEffortId, 
        [FromBody] MultiPaymentCertificateDto certificate)
    {
        var result = await Mediator.Send(new UpdateMultiPaymentCertificate.Command 
        { 
            Certificate = certificate,
            WorkEffortId = workEffortId 
        });
        return HandleResult(result);
    }
    
    [HttpPost("approveMultiPaymentCertificate")]
    public async Task<ActionResult<MultiPaymentCertificateDto>> ApproveMultiPaymentCertificate([FromBody] ApproveMultiPaymentCertificateRequest request)
    {
        var result = await Mediator.Send(new ApproveMultiPaymentCertificate.Command 
        { 
            WorkEffortId = request.WorkEffortId, 
            CompanyId = request.CompanyId 
        });
        return HandleResult(result);
    }
}