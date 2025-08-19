namespace Application.Shipments;

public class ShipmentReceiptDTo
{
    public string ReceiptId { get; set; } = null!;
    public string? InventoryItemId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ShipmentId { get; set; }
    public string? ShipmentItemSeqId { get; set; }
    public string? ShipmentPackageSeqId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ReturnId { get; set; }
    public string? ReturnItemSeqId { get; set; }
    public string? RejectionId { get; set; }
    public string? ReceivedByUserLoginId { get; set; }
    public DateTime? DatetimeReceived { get; set; }
    public string? ItemDescription { get; set; }
    public decimal? QuantityAccepted { get; set; }
    public decimal? QuantityRejected { get; set; }
}