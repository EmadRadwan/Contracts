namespace Domain;

public class InventoryItemVariance
{
    public InventoryItemVariance()
    {
        AcctgTrans = new HashSet<AcctgTran>();
    }

    public string InventoryItemId { get; set; } = null!;
    public string PhysicalInventoryId { get; set; } = null!;
    public string? VarianceReasonId { get; set; }
    public decimal? AvailableToPromiseVar { get; set; }
    public decimal? QuantityOnHandVar { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public PhysicalInventory PhysicalInventory { get; set; } = null!;
    public VarianceReason? VarianceReason { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
}