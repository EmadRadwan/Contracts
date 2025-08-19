namespace Domain;

public class PartyCarrierAccount
{
    public string PartyId { get; set; } = null!;
    public string CarrierPartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AccountNumber { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party CarrierParty { get; set; } = null!;
    public Party Party { get; set; } = null!;
}