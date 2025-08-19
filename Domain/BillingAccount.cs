using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class BillingAccount
{
    public BillingAccount()
    {
        BillingAccountRoles = new HashSet<BillingAccountRole>();
        BillingAccountTerms = new HashSet<BillingAccountTerm>();
        Invoices = new HashSet<Invoice>();
        OrderHeaders = new HashSet<OrderHeader>();
        PaymentApplications = new HashSet<PaymentApplication>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ReturnItemResponses = new HashSet<ReturnItemResponse>();
    }

    public string BillingAccountId { get; set; } = null!;
    public decimal? AccountLimit { get; set; }
    public string? AccountCurrencyUomId { get; set; }
    public string? ContactMechId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
    public string? ExternalAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? AccountCurrencyUom { get; set; }
    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public ICollection<BillingAccountRole> BillingAccountRoles { get; set; }
    public ICollection<BillingAccountTerm> BillingAccountTerms { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<PaymentApplication> PaymentApplications { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<ReturnItemResponse> ReturnItemResponses { get; set; }
}