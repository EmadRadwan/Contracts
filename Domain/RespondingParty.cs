namespace Domain;

public class RespondingParty
{
    public string RespondingPartySeqId { get; set; } = null!;
    public string CustRequestId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string? ContactMechId { get; set; }
    public DateTime? DateSent { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public CustRequest CustRequest { get; set; } = null!;
    public Party Party { get; set; } = null!;
}