namespace Domain;

public class ContactListPartyStatus
{
    public string ContactListId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime StatusDate { get; set; }
    public string? StatusId { get; set; }
    public string? SetByUserLoginId { get; set; }
    public string? OptInVerifyCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactListParty ContactListParty { get; set; } = null!;
}