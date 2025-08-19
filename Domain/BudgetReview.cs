namespace Domain;

public class BudgetReview
{
    public string BudgetId { get; set; } = null!;
    public string BudgetReviewId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string BudgetReviewResultTypeId { get; set; } = null!;
    public DateTime? ReviewDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public BudgetReviewResultType BudgetReviewResultType { get; set; } = null!;
    public Party Party { get; set; } = null!;
}