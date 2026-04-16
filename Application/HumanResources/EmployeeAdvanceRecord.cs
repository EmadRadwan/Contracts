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
    public DateOnly? AdvanceDate { get; set; }
    public decimal? Amount { get; set; }
    public int? InstallmentCount { get; set; }
    public DateOnly? StartDate { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; } // ← Translated
    public string? Description { get; set; }
    public List<EmployeeAdvanceScheduleRecord> Schedules { get; set; } = new();

    // Audit
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
}

public class EmployeeAdvanceScheduleRecord
{
    public string ScheduleId { get; set; } = null!;
    public int InstallmentNumber { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal ScheduledAmount { get; set; }
    public decimal DeductedAmount { get; set; }
    public string? StatusId { get; set; }
    public string? PayrolInvoiceId { get; set; }
    public string? Notes { get; set; }
}