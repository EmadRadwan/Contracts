namespace Application.Shipments;

public class OrderItemShipGroupAssocDto
{
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public List<string> OrderItemSeqIds { get; set; }
    public decimal TotalQuantity { get; set; }
}