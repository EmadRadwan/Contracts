using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class EmployeeAdvance
{
    public EmployeeAdvance()
    {
        EmployeeAdvanceSchedules = new HashSet<EmployeeAdvanceSchedule>();
    }

    public string AdvanceId { get; set; } = null!;
    public string AdvanceTypeId { get; set; } = null!;
    
    public string PartyId { get; set; } = null!;
    public string? PaymentId { get; set; }
    public DateTime AdvanceDate { get; set; }
    public decimal Amount { get; set; }
    public int? InstallmentCount { get; set; }
    public DateTime? StartDate { get; set; }
    public string StatusId { get; set; } 
    public string? Description { get; set; }

    // Audit fields
    public DateTime CreatedStamp { get; set; }
    public DateTime CreatedTxStamp { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime LastUpdatedTxStamp { get; set; }

    // Navigation properties
    public virtual Party Party { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual ICollection<EmployeeAdvanceSchedule> EmployeeAdvanceSchedules { get; set; }
}