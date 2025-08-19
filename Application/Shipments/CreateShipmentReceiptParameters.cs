namespace Application.Shipments;

public class CreateShipmentReceiptParameters
{
    public string InventoryItemId { get; set; }
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }
    public decimal QuantityAccepted { get; set; }
    public string FacilityId { get; set; }
    public string ShipmentId { get; set; }
    public string ShipmentItemSeqId { get; set; }
    public string ReturnId { get; set; }
    public string ReturnItemSeqId { get; set; }
    public decimal? QuantityRejected { get; set; }
    public string InventoryItemDetailSeqId { get; set; }
}
