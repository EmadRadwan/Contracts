namespace Domain;

public class InventoryItemLabelAppl
{
    public string InventoryItemId { get; set; } = null!;
    public string InventoryItemLabelTypeId { get; set; } = null!;
    public string? InventoryItemLabelId { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public InventoryItemLabel? InventoryItemLabel { get; set; }
    public InventoryItemLabelType InventoryItemLabelType { get; set; } = null!;
}