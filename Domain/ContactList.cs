namespace Domain;

public class ContactList
{
    public ContactList()
    {
        CommunicationEvents = new HashSet<CommunicationEvent>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        ContactListParties = new HashSet<ContactListParty>();
        WebSiteContactLists = new HashSet<WebSiteContactList>();
    }

    public string ContactListId { get; set; } = null!;
    public string? ContactListTypeId { get; set; }
    public string? ContactMechTypeId { get; set; }
    public string? MarketingCampaignId { get; set; }
    public string? ContactListName { get; set; }
    public string? Description { get; set; }
    public string? Comments { get; set; }
    public string? IsPublic { get; set; }
    public string? SingleUse { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? VerifyEmailFrom { get; set; }
    public string? VerifyEmailScreen { get; set; }
    public string? VerifyEmailSubject { get; set; }
    public string? VerifyEmailWebSiteId { get; set; }
    public string? OptOutScreen { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactListType? ContactListType { get; set; }
    public ContactMechType? ContactMechType { get; set; }
    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public MarketingCampaign? MarketingCampaign { get; set; }
    public Party? OwnerParty { get; set; }
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<ContactListParty> ContactListParties { get; set; }
    public ICollection<WebSiteContactList> WebSiteContactLists { get; set; }
}