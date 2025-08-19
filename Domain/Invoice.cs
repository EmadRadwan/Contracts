using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Invoice
{
    public Invoice()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        InvoiceAttributes = new HashSet<InvoiceAttribute>();
        InvoiceContactMeches = new HashSet<InvoiceContactMech>();
        InvoiceContents = new HashSet<InvoiceContent>();
        InvoiceItems = new HashSet<InvoiceItem>();
        InvoiceNotes = new HashSet<InvoiceNote>();
        InvoiceRoles = new HashSet<InvoiceRole>();
        InvoiceStatuses = new HashSet<InvoiceStatus>();
        InvoiceTerms = new HashSet<InvoiceTerm>();
        PaymentApplications = new HashSet<PaymentApplication>();
    }

    public string InvoiceId { get; set; } = null!;
    public string? InvoiceTypeId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? BillingAccountId { get; set; }
    public string? ContactMechId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? InvoiceMessage { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccount? BillingAccount { get; set; }
    public ContactMech? ContactMech { get; set; }
    public Uom? CurrencyUom { get; set; }
    public InvoiceType? InvoiceType { get; set; }
    public Party? Party { get; set; }
    public Party? PartyIdFromNavigation { get; set; }
    public RecurrenceInfo? RecurrenceInfo { get; set; }
    public RoleType? RoleType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<InvoiceAttribute> InvoiceAttributes { get; set; }
    public ICollection<InvoiceContactMech> InvoiceContactMeches { get; set; }
    public ICollection<InvoiceContent> InvoiceContents { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<InvoiceNote> InvoiceNotes { get; set; }
    public ICollection<InvoiceRole> InvoiceRoles { get; set; }
    public ICollection<InvoiceStatus> InvoiceStatuses { get; set; }
    public ICollection<InvoiceTerm> InvoiceTerms { get; set; }
    public ICollection<PaymentApplication> PaymentApplications { get; set; }
}