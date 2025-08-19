namespace Domain;

public class ContactListType
{
    public ContactListType()
    {
        ContactLists = new HashSet<ContactList>();
    }

    public string ContactListTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContactList> ContactLists { get; set; }
}