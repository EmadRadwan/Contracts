namespace Domain;

public class InvoiceNote
{
    public string InvoiceId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public NoteDatum Note { get; set; } = null!;
}