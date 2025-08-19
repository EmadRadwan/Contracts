namespace Domain;

public class PartyIcsAvsOverride
{
    public string PartyId { get; set; } = null!;
    public string? AvsDeclineString { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
}