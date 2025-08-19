namespace Application.Facilities;

public class PicklistItemDto
{
    public string PicklistBinId { get; set; }
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
}
