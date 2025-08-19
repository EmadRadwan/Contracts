namespace Domain;

public class ContactMechLink
{
    public string ContactMechIdFrom { get; set; } = null!;
    public string ContactMechIdTo { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMechIdFromNavigation { get; set; } = null!;
    public ContactMech ContactMechIdToNavigation { get; set; } = null!;
}