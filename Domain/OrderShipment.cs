namespace Domain;

public class OrderShipment
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string ShipGroupSeqId { get; set; } = null!;
    public string ShipmentId { get; set; } = null!;
    public string ShipmentItemSeqId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public Shipment Shipment { get; set; } = null!;
}