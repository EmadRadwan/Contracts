namespace Domain;

public class ContactListParty
{
    public ContactListParty()
    {
        ContactListPartyStatuses = new HashSet<ContactListPartyStatus>();
    }

    public string ContactListId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public string? PreferredContactMechId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactList ContactList { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public ContactMech? PreferredContactMech { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<ContactListPartyStatus> ContactListPartyStatuses { get; set; }
}