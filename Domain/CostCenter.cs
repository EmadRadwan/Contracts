using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

/// <summary>
/// New entity: CostCenter
/// One CostCenter can be linked to many PaymentTypes
/// </summary>
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class CostCenter
{
    public CostCenter()
    {
        Payments = new HashSet<Payment>();
        WorkEfforts = new HashSet<WorkEffort>();
        AcctgTrans = new HashSet<AcctgTran>();
    }

    public string CostCenterId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string IsOutPayment { get; set; } = null!;

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public virtual ICollection<Payment> Payments { get; set; }
    
    public virtual ICollection<WorkEffort> WorkEfforts { get; set; } = new HashSet<WorkEffort>();
    public virtual ICollection<AcctgTran> AcctgTrans { get; set; }
}