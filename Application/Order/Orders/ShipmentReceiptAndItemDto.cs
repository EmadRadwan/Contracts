namespace Application.Order.Orders;

public class ShipmentReceiptAndItemDto
{
    // From ShipmentReceipt (SR)
    public string ReceiptId { get; set; }
    public string ShipmentId { get; set; }
    public DateTime? DatetimeReceived { get; set; }
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal? QuantityRejected { get; set; }
    public decimal? QuantityAccepted { get; set; }

    // From InventoryItem (II)
    public string FacilityId { get; set; }
    public string LocationSeqId { get; set; }
    public double QuantityOnHandTotal { get; set; }
    public double AvailableToPromiseTotal { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public string LotId { get; set; }
}
