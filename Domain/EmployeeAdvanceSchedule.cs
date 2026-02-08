using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class EmployeeAdvanceSchedule
{
    public string ScheduleId { get; set; } = null!;
    public string AdvanceId { get; set; } = null!;
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal ScheduledAmount { get; set; }
    public decimal DeductedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string StatusId { get; set; } = "SCHEDULE_PENDING";
    public string? PayrolInvoiceId { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedStamp { get; set; }
    public DateTime CreatedTxStamp { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime LastUpdatedTxStamp { get; set; }

    // Navigation properties
    public virtual EmployeeAdvance EmployeeAdvance { get; set; } = null!;
    public virtual Invoice? PayrolInvoice { get; set; }
}