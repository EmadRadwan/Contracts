namespace Domain;

public class WorkEffortInventoryProduced
{
    public string WorkEffortId { get; set; } = null!;
    public string InventoryItemId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}