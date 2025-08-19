namespace Domain;

public class GlReconciliationEntry
{
    public string GlReconciliationId { get; set; } = null!;
    public string AcctgTransId { get; set; } = null!;
    public string AcctgTransEntrySeqId { get; set; } = null!;
    public decimal? ReconciledAmount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AcctgTransEntry AcctgTrans { get; set; } = null!;
    public GlReconciliation GlReconciliation { get; set; } = null!;
}