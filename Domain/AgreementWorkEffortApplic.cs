namespace Domain;

public class AgreementWorkEffortApplic
{
    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Agreement Agreement { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}