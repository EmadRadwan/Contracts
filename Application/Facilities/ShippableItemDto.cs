namespace Application.Facilities;

public class ShippableItemDto
{
    public string OrderId { get; set; }
    public string OrderItemSeqId { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string InventoryProductId { get; set; }
    public string InventoryItemId { get; set; }
    public string PicklistBinId { get; set; }
    public string ShipGroupSeqId { get; set; }

    public decimal QuantityOrdered { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotQuantityReserved { get; set; }
    public decimal TotQuantityNotAvailable { get; set; }
    public decimal TotQuantityAvailable { get; set; }
}
