namespace Domain;

public class PartyNote
{
    public string PartyId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public NoteDatum Note { get; set; } = null!;
    public Party Party { get; set; } = null!;
}