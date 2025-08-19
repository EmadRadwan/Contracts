namespace Domain;

public class BudgetAttribute
{
    public string BudgetId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
}