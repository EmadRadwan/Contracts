namespace Domain;

public class PartyIdentification
{
    public string PartyId { get; set; } = null!;
    public string PartyIdentificationTypeId { get; set; } = null!;
    public string? IdValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyIdentificationType PartyIdentificationType { get; set; } = null!;
}