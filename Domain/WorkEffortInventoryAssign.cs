namespace Domain;

public class WorkEffortInventoryAssign
{
    public string WorkEffortId { get; set; } = null!;
    public string InventoryItemId { get; set; } = null!;
    public string? StatusId { get; set; }
    public double? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public WorkEffort WorkEffort { get; set; } = null!;
}