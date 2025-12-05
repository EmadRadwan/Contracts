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
    }

    public string CostCenterId { get; set; } = null!;      // PK
    public string Description { get; set; } = null!;       // Human readable description
    public string IsOutPayment { get; set; } = null!;       

    // Timestamps (kept for consistency with the rest of the model)
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }

}