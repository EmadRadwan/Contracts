namespace Domain;

public class ContactListCommStatus
{
    public string ContactListId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public string? PartyId { get; set; }
    public string? MessageId { get; set; }
    public string? StatusId { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public ContactList ContactList { get; set; } = null!;
    public ContactMech ContactMech { get; set; } = null!;
    public Party? Party { get; set; }
    public StatusItem? Status { get; set; }
}