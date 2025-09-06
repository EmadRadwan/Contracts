
namespace Application.Projects;

public class ProjectCertificateDto
{
    public string? WorkEffortId { get; set; } // Maps to WorkEffort.WorkEffortId
    public string? CertificateNumber { get; set; } // Maps to WorkEffort.CertificateNumber
    public string? WorkEffortTypeId { get; set; } // Maps to WorkEffort.WorkEffortTypeId
    public string? ProjectId { get; set; } // Maps to WorkEffort.ProjectNum
    public string? CertificateCategory { get; set; } // Maps to WorkEffort.ProjectName
    public string? PartyId { get; set; } // Maps to WorkEffort.PartyId
    public string? Description { get; set; } // Maps to WorkEffort.Description
    public DateTime? EstimatedStartDate { get; set; } // Maps to WorkEffort.EstimatedStartDate (ISO string)
    public DateTime? EstimatedCompletionDate { get; set; } // Maps to WorkEffort.EstimatedCompletionDate (ISO string)
    public string? StatusDescription { get; set; } // Maps to WorkEffort.StatusDescription
    public List<CertificateItemDto>? CertificateItems { get; set; } // List of associated items
}