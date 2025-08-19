namespace Application.Shipments;

public class IssueOrderItemShipGrpInvResParameters
{
    public string ShipmentId { get; set; }
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string? InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime? EventDate { get; set; }
}