using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ShipmentItemBilling
{
    public string ShipmentId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public string InvoiceId { get; set; } = null!;
    public string InvoiceItemSeqId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem InvoiceI { get; set; } = null!;
    public ShipmentItem ShipmentI { get; set; } = null!;
}