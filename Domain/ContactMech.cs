using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ContactMech
{
    public ContactMech()
    {
        BillingAccounts = new HashSet<BillingAccount>();
        CheckAccounts = new HashSet<CheckAccount>();
        CommunicationEventContactMechIdFromNavigations = new HashSet<CommunicationEvent>();
        CommunicationEventContactMechIdToNavigations = new HashSet<CommunicationEvent>();
        CommunicationEventRoles = new HashSet<CommunicationEventRole>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        ContactListParties = new HashSet<ContactListParty>();
        ContactMechAttributes = new HashSet<ContactMechAttribute>();
        ContactMechLinkContactMechIdFromNavigations = new HashSet<ContactMechLink>();
        ContactMechLinkContactMechIdToNavigations = new HashSet<ContactMechLink>();
        CreditCards = new HashSet<CreditCard>();
        CustRequests = new HashSet<CustRequest>();
        EftAccounts = new HashSet<EftAccount>();
        FacilityContactMechPurposes = new HashSet<FacilityContactMechPurpose>();
        FacilityContactMeches = new HashSet<FacilityContactMech>();
        GiftCards = new HashSet<GiftCard>();
        InvoiceContactMeches = new HashSet<InvoiceContactMech>();
        Invoices = new HashSet<Invoice>();
        OrderContactMeches = new HashSet<OrderContactMech>();
        OrderItemContactMeches = new HashSet<OrderItemContactMech>();
        OrderItemShipGroupContactMeches = new HashSet<OrderItemShipGroup>();
        OrderItemShipGroupTelecomContactMeches = new HashSet<OrderItemShipGroup>();
        PartyContactMechPurposes = new HashSet<PartyContactMechPurpose>();
        PartyContactMeches = new HashSet<PartyContactMech>();
        PayPalPaymentMethods = new HashSet<PayPalPaymentMethod>();
        ProdPromoCodeContactMeches = new HashSet<ProdPromoCodeContactMech>();
        RespondingParties = new HashSet<RespondingParty>();
        ReturnContactMeches = new HashSet<ReturnContactMech>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ShipmentContactMeches = new HashSet<ShipmentContactMech>();
        ShoppingLists = new HashSet<ShoppingList>();
        Subscriptions = new HashSet<Subscription>();
        WorkEffortContactMechNews = new HashSet<WorkEffortContactMechNew>();
        WorkEffortEventReminders = new HashSet<WorkEffortEventReminder>();
    }

    public string ContactMechId { get; set; } = null!;
    public string? ContactMechTypeId { get; set; }
    public string? InfoString { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMechType? ContactMechType { get; set; }
    public FtpAddress FtpAddress { get; set; } = null!;
    public PostalAddress PostalAddress { get; set; } = null!;
    public TelecomNumber TelecomNumber { get; set; } = null!;
    public ICollection<BillingAccount> BillingAccounts { get; set; }
    public ICollection<CheckAccount> CheckAccounts { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventContactMechIdFromNavigations { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventContactMechIdToNavigations { get; set; }
    public ICollection<CommunicationEventRole> CommunicationEventRoles { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<ContactListParty> ContactListParties { get; set; }
    public ICollection<ContactMechAttribute> ContactMechAttributes { get; set; }
    public ICollection<ContactMechLink> ContactMechLinkContactMechIdFromNavigations { get; set; }
    public ICollection<ContactMechLink> ContactMechLinkContactMechIdToNavigations { get; set; }
    public ICollection<CreditCard> CreditCards { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<EftAccount> EftAccounts { get; set; }
    public ICollection<FacilityContactMechPurpose> FacilityContactMechPurposes { get; set; }
    public ICollection<FacilityContactMech> FacilityContactMeches { get; set; }
    public ICollection<GiftCard> GiftCards { get; set; }
    public ICollection<InvoiceContactMech> InvoiceContactMeches { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<OrderContactMech> OrderContactMeches { get; set; }
    public ICollection<OrderItemContactMech> OrderItemContactMeches { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroupContactMeches { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroupTelecomContactMeches { get; set; }
    public ICollection<PartyContactMechPurpose> PartyContactMechPurposes { get; set; }
    public ICollection<PartyContactMech> PartyContactMeches { get; set; }
    public ICollection<PayPalPaymentMethod> PayPalPaymentMethods { get; set; }
    public ICollection<ProdPromoCodeContactMech> ProdPromoCodeContactMeches { get; set; }
    public ICollection<RespondingParty> RespondingParties { get; set; }
    public ICollection<ReturnContactMech> ReturnContactMeches { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<ShipmentContactMech> ShipmentContactMeches { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<WorkEffortContactMechNew> WorkEffortContactMechNews { get; set; }
    public ICollection<WorkEffortEventReminder> WorkEffortEventReminders { get; set; }
}