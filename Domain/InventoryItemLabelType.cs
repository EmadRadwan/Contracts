namespace Domain;

public class InventoryItemLabelType
{
    public InventoryItemLabelType()
    {
        InventoryItemLabelAppls = new HashSet<InventoryItemLabelAppl>();
        InventoryItemLabels = new HashSet<InventoryItemLabel>();
        InverseParentType = new HashSet<InventoryItemLabelType>();
    }

    public string InventoryItemLabelTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItemLabelType? ParentType { get; set; }
    public ICollection<InventoryItemLabelAppl> InventoryItemLabelAppls { get; set; }
    public ICollection<InventoryItemLabel> InventoryItemLabels { get; set; }
    public ICollection<InventoryItemLabelType> InverseParentType { get; set; }
}