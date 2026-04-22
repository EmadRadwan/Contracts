using Application.Core;
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
    
    [HttpPost("{workEffortId}/approve-po", Name = "ApprovePOForCertificate")]
    public async Task<ActionResult> ApprovePurchaseOrder(string workEffortId)
    {
        var result = await Mediator.Send(new ApprovePOForCertificate.Command 
        { 
            WorkEffortId = workEffortId 
        });

        return HandleResult(result);
    }
    
    [HttpPut("updateProjectCertificate/{workEffortId}", Name = "UpdateProjectCertificate")]
    public async Task<ActionResult<ProjectCertificateDto>> UpdateProjectCertificate(string workEffortId, [FromBody] ProjectCertificateDto certificate)
    {
        certificate.WorkEffortId = workEffortId;
        var result = await Mediator.Send(new UpdateProjectCertificate.Command { Certificate = certificate });
        return HandleResult(result);
    }
    
    [HttpDelete("deleteProjectCertificate/{workEffortId}", Name = "DeleteProjectCertificate")]
    public async Task<ActionResult> DeleteProjectCertificate(string workEffortId)
    {
        var result = await Mediator.Send(new DeleteProjectCertificate.Command 
        { 
            WorkEffortId = workEffortId 
        });

        if (result.IsSuccess)
        {
            return Ok(new { message = "Certificate and related artifacts deleted successfully" });
        }

        return HandleResult(result); // Assuming you have a method like this
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
    
    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetProjectCertificatesByDateRange(
        [FromQuery] DateTime? startDate, 
        [FromQuery] DateTime? endDate)
    {
        var language = GetLanguage();

        var list = await Mediator.Send(new ListProjectCertificatesByDateRange.Query
        {
            StartDate = startDate,
            EndDate   = endDate,
            Language  = language
        });

        // Option A – if you use Ardalis.Result / similar
        return HandleResult(Result<List<ProjectCertificateRecord>>.Success(list));
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

    [HttpPost("{id}/duplicateMultiPaymentCertificate")]
    public async Task<ActionResult<MultiPaymentCertificateDto>> DuplicateMultiPaymentCertificate(string id)
    {
        var result = await Mediator.Send(new DuplicateMultiPaymentCertificate.Command { OriginalWorkEffortId = id });
        return HandleResult(result);
    }

    [HttpDelete("multiPaymentCertificate/{workEffortId}")]
    public async Task<ActionResult> DeleteMultiPaymentCertificate(string workEffortId)
    {
        return HandleResult(await Mediator.Send(new DeleteMultiPaymentCertificate.Command { WorkEffortId = workEffortId }));
    }

    [HttpPost("resetMultiPaymentCertificate/{workEffortId}")]
    public async Task<ActionResult<MultiPaymentCertificateDto>> ResetMultiPaymentCertificate(string workEffortId)
    {
        return HandleResult(await Mediator.Send(new ResetMultiPaymentCertificate.Command { WorkEffortId = workEffortId }));
    }

    [HttpGet("multiPaymentCertificates/by-date-range")]
    public async Task<IActionResult> GetMultiPaymentCertificatesByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var language = GetLanguage();
        var result = await Mediator.Send(new ListMultiPaymentCertificatesByDateRange.Query
        {
            StartDate = startDate,
            EndDate = endDate,
            Language = language
        });
        return HandleResult(result);
    }

    [HttpGet("multiPaymentItems/by-date-range")]
    public async Task<IActionResult> GetMultiPaymentItemsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await Mediator.Send(new ListMultiPaymentItemsByDateRange.Query
        {
            StartDate = startDate,
            EndDate = endDate
        });
        return HandleResult(result);
    }
    
    [HttpPost("review", Name = "ReviewProjectCertificate")]
    public async Task<ActionResult<ProjectCertificateDto>> ReviewProjectCertificate([FromBody] ReviewProjectCertificate.Command command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("resetCertificate/{workEffortId}", Name = "ResetProjectCertificate")]
    public async Task<ActionResult> ResetProjectCertificate(string workEffortId)
    {
        var result = await Mediator.Send(new ResetProjectCertificate.Command { WorkEffortId = workEffortId });
        return HandleResult(result);
    }

    [HttpGet("workEffortsByGlAccount")]
    public async Task<IActionResult> GetWorkEffortsByGlAccount(
        [FromQuery] string? glAccountId,
        [FromQuery] string? workEffortTypeId,
        [FromQuery] string? workEffortParentId)
    {
        return HandleResult(await Mediator.Send(new ListWorkEffortsByGlAccount.Query
        {
            GlAccountId = glAccountId,
            WorkEffortTypeId = workEffortTypeId,
            WorkEffortParentId = workEffortParentId
        }));
    }

    [HttpGet("report")]
    public async Task<ActionResult<ProjectReportDto>> GetProjectReport(
        [FromQuery] string projectId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] bool allData)
    {
        return HandleResult(Result<ProjectReportDto>.Success(await Mediator.Send(new GetProjectReport.Query
        {
            ProjectId = projectId,
            StartDate = startDate,
            EndDate = endDate,
            AllData = allData
        })));
    }
    
}

