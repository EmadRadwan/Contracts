namespace Application.Facilities;

public class PackingItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string InventoryItemId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ShippedQuantity { get; set; }
    public decimal QuantityToShip { get; set; }
    public bool IncludeThisItem { get; set; }
}