namespace Application.Facilities;

public class ShippingItemDto
{
    public string ProductId { get; set; }  // Product being shipped
    public string OrderId { get; set; }    // Order associated with the item
    public string OrderItemSeqId { get; set; } // Unique identifier for order line
    public string InventoryItemId { get; set; } // Inventory tracking (if needed)
    
    public decimal OrderedQuantity { get; set; }  // Total quantity ordered
    public decimal ShippedQuantity { get; set; }  // Quantity that has already been shipped
    public decimal RemainingQuantity => OrderedQuantity - ShippedQuantity; // To be shipped
}
