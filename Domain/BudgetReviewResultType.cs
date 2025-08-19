namespace Domain;

public class BudgetReviewResultType
{
    public BudgetReviewResultType()
    {
        BudgetReviews = new HashSet<BudgetReview>();
    }

    public string BudgetReviewResultTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<BudgetReview> BudgetReviews { get; set; }
}