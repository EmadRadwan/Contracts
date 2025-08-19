namespace Domain;

public class CustRequestItemWorkEffort
{
    public string CustRequestId { get; set; } = null!;
    public string CustRequestItemSeqId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequestItem CustRequestI { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}