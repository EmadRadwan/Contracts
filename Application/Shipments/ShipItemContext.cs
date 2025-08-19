namespace Application.Shipments;

public class ShipItemContext
{
    public string ShipmentId { get; set; }
    public string ShipmentItemSeqId { get; set; }
    public decimal Quantity { get; set; }
    public string ShipmentPackageSeqId { get; set; }
}