namespace Domain;

public class ShoppingListWorkEffort
{
    public string ShoppingListId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShoppingList ShoppingList { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}