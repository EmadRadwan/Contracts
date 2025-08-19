namespace Domain;

public class WorkEffortReview
{
    public string WorkEffortId { get; set; } = null!;
    public string UserLoginId { get; set; } = null!;
    public DateTime ReviewDate { get; set; }
    public string? StatusId { get; set; }
    public string? PostedAnonymous { get; set; }
    public double? Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem? Status { get; set; }
    public UserLogin UserLogin { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}