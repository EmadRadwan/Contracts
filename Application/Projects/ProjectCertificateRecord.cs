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
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string? StatusDescription { get; set; }

    public string? CurrentStatusId { get; set; }

    // REFACTOR: Renamed SupplierPartyId to PartyIdSupplier
    // Purpose: Align with WorkEffort table and frontend SelectedCertificate interface
    // Context: Fixes binding issue in ProjectCertificateForm ComboBox
    public string? PartyIdSupplier { get; set; }

    // REFACTOR: Renamed SupplierPartyName to PartyNameSupplier
    public string? PartyNameSupplier { get; set; }

    // REFACTOR: Renamed ContractorPartyId to PartyIdContractor
    public string? PartyIdContractor { get; set; }

    // REFACTOR: Renamed ContractorPartyName to PartyNameContractor
    public string? PartyNameContractor { get; set; }
    public string? RelatedOrderId { get; set; }
}