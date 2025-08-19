using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderItemBilling
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string InvoiceId { get; set; } = null!;
    public string InvoiceItemSeqId { get; set; } = null!;
    public string? ItemIssuanceId { get; set; }
    public string? ShipmentReceiptId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem InvoiceI { get; set; } = null!;
    public ItemIssuance? ItemIssuance { get; set; }
    public OrderHeader Order { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
    public ShipmentReceipt? ShipmentReceipt { get; set; }
}