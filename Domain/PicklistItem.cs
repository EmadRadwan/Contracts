namespace Domain;

public class PicklistItem
{
    public string PicklistBinId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string ShipGroupSeqId { get; set; } = null!;
    public string InventoryItemId { get; set; } = null!;
    public string? ItemStatusId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public StatusItem? ItemStatus { get; set; }
    public OrderItem OrderI { get; set; } = null!;
    public OrderItemShipGroup OrderItemShipGroup { get; set; } = null!;
    public PicklistBin PicklistBin { get; set; } = null!;
}