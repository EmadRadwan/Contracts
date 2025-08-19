namespace Domain;

public class CommunicationEventType
{
    public CommunicationEventType()
    {
        CommunicationEvents = new HashSet<CommunicationEvent>();
        InverseParentType = new HashSet<CommunicationEventType>();
    }

    public string CommunicationEventTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? ContactMechTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechType? ContactMechType { get; set; }
    public CommunicationEventType? ParentType { get; set; }
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<CommunicationEventType> InverseParentType { get; set; }
}