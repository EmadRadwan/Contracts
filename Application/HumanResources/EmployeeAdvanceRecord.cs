using System.ComponentModel.DataAnnotations;

namespace Application.HumanResources;

public class EmployeeAdvanceRecord
{
    [Key] public string AdvanceId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string? EmployeeName { get; set; } // ← Full name
    public string? AdvanceTypeId { get; set; }
    public string? AdvanceTypeDescription { get; set; }
    public string? PaymentId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal? Amount { get; set; }
    public int? InstallmentCount { get; set; }
    public DateTime? StartDate { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; } // ← Translated
    public string? Description { get; set; }

    // Audit
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
}