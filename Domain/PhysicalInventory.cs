namespace Domain;

public class PhysicalInventory
{
    public PhysicalInventory()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        InventoryItemVariances = new HashSet<InventoryItemVariance>();
    }

    public string PhysicalInventoryId { get; set; } = null!;
    public DateTime? PhysicalInventoryDate { get; set; }
    public string? PartyId { get; set; }
    public string? GeneralComments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<InventoryItemVariance> InventoryItemVariances { get; set; }
}