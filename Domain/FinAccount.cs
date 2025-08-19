using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FinAccount
{
    public FinAccount()
    {
        FinAccountAttributes = new HashSet<FinAccountAttribute>();
        FinAccountAuths = new HashSet<FinAccountAuth>();
        FinAccountRoles = new HashSet<FinAccountRole>();
        FinAccountStatuses = new HashSet<FinAccountStatus>();
        FinAccountTrans = new HashSet<FinAccountTran>();
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        PaymentMethods = new HashSet<PaymentMethod>();
        ReturnHeaders = new HashSet<ReturnHeader>();
    }

    public string FinAccountId { get; set; } = null!;
    public string? FinAccountTypeId { get; set; }
    public string? StatusId { get; set; }
    public string? FinAccountName { get; set; }
    public string? FinAccountCode { get; set; }
    public string? FinAccountPin { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? PostToGlAccountId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsRefundable { get; set; }
    public string? ReplenishPaymentId { get; set; }
    public decimal? ReplenishLevel { get; set; }
    public decimal? ActualBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public FinAccountType? FinAccountType { get; set; }
    public Party? OrganizationParty { get; set; }
    public Party? OwnerParty { get; set; }
    public GlAccount? PostToGlAccount { get; set; }
    public PaymentMethod? ReplenishPayment { get; set; }
    public ICollection<FinAccountAttribute> FinAccountAttributes { get; set; }
    public ICollection<FinAccountAuth> FinAccountAuths { get; set; }
    public ICollection<FinAccountRole> FinAccountRoles { get; set; }
    public ICollection<FinAccountStatus> FinAccountStatuses { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<PaymentMethod> PaymentMethods { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
}