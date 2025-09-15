namespace Application.Projects;

public class ProjectCertificateDto
{
    public string? WorkEffortId { get; set; }
    public string? CertificateNumber { get; set; }
    public string? WorkEffortTypeId { get; set; }
    public string? CertificateCategory { get; set; }
    public string? CurrentStatusId { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; } // Added for frontend display
    public string? PartyIdSupplier { get; set; }
    public string? PartyNameSupplier { get; set; } // Added for frontend display
    public string? PartyIdContractor { get; set; }
    public string? PartyNameContractor { get; set; } // Added for frontend display
    public string? RelatedOrderId { get; set; }
    public string? Description { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string? StatusDescription { get; set; }
    public string? StatusDescriptionArabic { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public List<CertificateItemDto>? CertificateItems { get; set; } // List of associated items
}