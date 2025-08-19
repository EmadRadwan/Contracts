namespace Domain;

public class CustRequestItemNote
{
    public string CustRequestId { get; set; } = null!;
    public string CustRequestItemSeqId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustRequestItem CustRequestI { get; set; } = null!;
    public NoteDatum Note { get; set; } = null!;
}