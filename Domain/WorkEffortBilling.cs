namespace Domain;

public class WorkEffortBilling
{
    public string WorkEffortId { get; set; } = null!;
    public string InvoiceId { get; set; } = null!;
    public string InvoiceItemSeqId { get; set; } = null!;
    public double? Percentage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem InvoiceI { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}