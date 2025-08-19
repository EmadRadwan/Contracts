namespace Application.Facilities;

public class PackOrderItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string ProductId { get; set; }
    public string? InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Weight { get; set; }
    public int? PackageSeqId { get; set; }
}