namespace Domain;

public class QuoteNote
{
    public string QuoteId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public NoteDatum Note { get; set; } = null!;
    public Quote Quote { get; set; } = null!;
}