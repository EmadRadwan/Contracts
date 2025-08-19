namespace Domain;

public class InventoryItemLabel
{
    public InventoryItemLabel()
    {
        InventoryItemLabelAppls = new HashSet<InventoryItemLabelAppl>();
    }

    public string InventoryItemLabelId { get; set; } = null!;
    public string? InventoryItemLabelTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItemLabelType? InventoryItemLabelType { get; set; }
    public ICollection<InventoryItemLabelAppl> InventoryItemLabelAppls { get; set; }
}