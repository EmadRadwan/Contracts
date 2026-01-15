using System.ComponentModel.DataAnnotations;

namespace Application.Projects;

public class ProjectCertificateRecord
{
    [Key] public string WorkEffortId { get; set; }
    public string? CertificateNumber { get; set; }
    public string? CertificateCategory { get; set; }
    public string? CertificateCategoryDescription { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string? StatusDescription { get; set; }

    public string? CurrentStatusId { get; set; }

    public string? PartyIdSupplier { get; set; }

    public string? PartyNameSupplier { get; set; }

    public string? PartyIdContractor { get; set; }

    public string? PartyNameContractor { get; set; }
    public string? RelatedOrderId { get; set; }
    public string? FacilityId { get; set; }
    public string FacilityName { get; set; }
    public decimal TotalAmount { get; set; }

}