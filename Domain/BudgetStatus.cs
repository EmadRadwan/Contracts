namespace Domain;

public class BudgetStatus
{
    public string BudgetId { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public DateTime? StatusDate { get; set; }
    public string? Comments { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public UserLogin? ChangeByUserLogin { get; set; }
    public StatusItem Status { get; set; } = null!;
}