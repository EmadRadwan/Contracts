using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class VarianceReason
{
    public VarianceReason()
    {
        InventoryItemVariances = new HashSet<InventoryItemVariance>();
        VarianceReasonGlAccounts = new HashSet<VarianceReasonGlAccount>();
    }

    public string VarianceReasonId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<InventoryItemVariance> InventoryItemVariances { get; set; }
    public ICollection<VarianceReasonGlAccount> VarianceReasonGlAccounts { get; set; }
}