namespace Domain;

public class CommunicationEvent
{
    public CommunicationEvent()
    {
        CommEventContentAssocs = new HashSet<CommEventContentAssoc>();
        CommunicationEventOrders = new HashSet<CommunicationEventOrder>();
        CommunicationEventProducts = new HashSet<CommunicationEventProduct>();
        CommunicationEventPurposes = new HashSet<CommunicationEventPurpose>();
        CommunicationEventReturns = new HashSet<CommunicationEventReturn>();
        CommunicationEventRoles = new HashSet<CommunicationEventRole>();
        CommunicationEventWorkEffs = new HashSet<CommunicationEventWorkEff>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        CustRequestCommEvents = new HashSet<CustRequestCommEvent>();
        PartyNeeds = new HashSet<PartyNeed>();
        SubscriptionCommEvents = new HashSet<SubscriptionCommEvent>();
    }

    public string CommunicationEventId { get; set; } = null!;
    public string? CommunicationEventTypeId { get; set; }
    public string? OrigCommEventId { get; set; }
    public string? ParentCommEventId { get; set; }
    public string? StatusId { get; set; }
    public string? ContactMechTypeId { get; set; }
    public string? ContactMechIdFrom { get; set; }
    public string? ContactMechIdTo { get; set; }
    public string? RoleTypeIdFrom { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? DatetimeStarted { get; set; }
    public DateTime? DatetimeEnded { get; set; }
    public string? Subject { get; set; }
    public string? ContentMimeTypeId { get; set; }
    public string? Content { get; set; }
    public string? Note { get; set; }
    public string? ReasonEnumId { get; set; }
    public string? ContactListId { get; set; }
    public string? HeaderString { get; set; }
    public string? FromString { get; set; }
    public string? ToString { get; set; }
    public string? CcString { get; set; }
    public string? BccString { get; set; }
    public string? MessageId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEventType? CommunicationEventType { get; set; }
    public ContactList? ContactList { get; set; }
    public ContactMech? ContactMechIdFromNavigation { get; set; }
    public ContactMech? ContactMechIdToNavigation { get; set; }
    public ContactMechType? ContactMechType { get; set; }
    public MimeType? ContentMimeType { get; set; }
    public Party? PartyIdFromNavigation { get; set; }
    public Party? PartyIdToNavigation { get; set; }
    public Enumeration? ReasonEnum { get; set; }
    public RoleType? RoleTypeIdFromNavigation { get; set; }
    public RoleType? RoleTypeIdToNavigation { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<CommEventContentAssoc> CommEventContentAssocs { get; set; }
    public ICollection<CommunicationEventOrder> CommunicationEventOrders { get; set; }
    public ICollection<CommunicationEventProduct> CommunicationEventProducts { get; set; }
    public ICollection<CommunicationEventPurpose> CommunicationEventPurposes { get; set; }
    public ICollection<CommunicationEventReturn> CommunicationEventReturns { get; set; }
    public ICollection<CommunicationEventRole> CommunicationEventRoles { get; set; }
    public ICollection<CommunicationEventWorkEff> CommunicationEventWorkEffs { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<CustRequestCommEvent> CustRequestCommEvents { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<SubscriptionCommEvent> SubscriptionCommEvents { get; set; }
}