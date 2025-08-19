namespace Domain;

public class QuoteWorkEffort
{
    public string QuoteId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Quote Quote { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}