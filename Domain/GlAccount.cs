using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccount
{
    public GlAccount()
    {
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        FinAccountTypeGlAccounts = new HashSet<FinAccountTypeGlAccount>();
        FinAccounts = new HashSet<FinAccount>();
        FixedAssetTypeGlAccountAccDepGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        FixedAssetTypeGlAccountAssetGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        FixedAssetTypeGlAccountDepGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        FixedAssetTypeGlAccountLossGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        FixedAssetTypeGlAccountProfitGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        GlAccountCategoryMembers = new HashSet<GlAccountCategoryMember>();
        GlAccountGroupMembers = new HashSet<GlAccountGroupMember>();
        GlAccountHistories = new HashSet<GlAccountHistory>();
        GlAccountOrganizations = new HashSet<GlAccountOrganization>();
        GlAccountRoles = new HashSet<GlAccountRole>();
        GlAccountTypeDefaults = new HashSet<GlAccountTypeDefault>();
        GlBudgetXrefs = new HashSet<GlBudgetXref>();
        GlReconciliations = new HashSet<GlReconciliation>();
        InverseParentGlAccount = new HashSet<GlAccount>();
        InvoiceItemTypeGlAccounts = new HashSet<InvoiceItemTypeGlAccount>();
        InvoiceItemTypes = new HashSet<InvoiceItemType>();
        InvoiceItems = new HashSet<InvoiceItem>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        OrderItems = new HashSet<OrderItem>();
        PartyGlAccounts = new HashSet<PartyGlAccount>();
        PaymentApplications = new HashSet<PaymentApplication>();
        PaymentMethodTypeGlAccounts = new HashSet<PaymentMethodTypeGlAccount>();
        PaymentMethodTypes = new HashSet<PaymentMethodType>();
        PaymentMethods = new HashSet<PaymentMethod>();
        Payments = new HashSet<Payment>();
        ProductCategoryGlAccounts = new HashSet<ProductCategoryGlAccount>();
        ProductGlAccounts = new HashSet<ProductGlAccount>();
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
        TaxAuthorityGlAccounts = new HashSet<TaxAuthorityGlAccount>();
        VarianceReasonGlAccounts = new HashSet<VarianceReasonGlAccount>();
    }

    public string GlAccountId { get; set; } = null!;
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? GlXbrlClassId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public string? AccountNameArabic { get; set; }
    public string? ProductId { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountClass? GlAccountClass { get; set; }
    public GlAccountType? GlAccountType { get; set; }
    public GlResourceType? GlResourceType { get; set; }
    public GlXbrlClass? GlXbrlClass { get; set; }
    public GlAccount? ParentGlAccount { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<FinAccountTypeGlAccount> FinAccountTypeGlAccounts { get; set; }
    public ICollection<FinAccount> FinAccounts { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccountAccDepGlAccounts { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccountAssetGlAccounts { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccountDepGlAccounts { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccountLossGlAccounts { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccountProfitGlAccounts { get; set; }
    public ICollection<GlAccountCategoryMember> GlAccountCategoryMembers { get; set; }
    public ICollection<GlAccountGroupMember> GlAccountGroupMembers { get; set; }
    public ICollection<GlAccountHistory> GlAccountHistories { get; set; }
    public ICollection<GlAccountOrganization> GlAccountOrganizations { get; set; }
    public ICollection<GlAccountRole> GlAccountRoles { get; set; }
    public ICollection<GlAccountTypeDefault> GlAccountTypeDefaults { get; set; }
    public ICollection<GlBudgetXref> GlBudgetXrefs { get; set; }
    public ICollection<GlReconciliation> GlReconciliations { get; set; }
    public ICollection<GlAccount> InverseParentGlAccount { get; set; }
    public ICollection<InvoiceItemTypeGlAccount> InvoiceItemTypeGlAccounts { get; set; }
    public ICollection<InvoiceItemType> InvoiceItemTypes { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<PartyGlAccount> PartyGlAccounts { get; set; }
    public ICollection<PaymentApplication> PaymentApplications { get; set; }
    public ICollection<PaymentMethodTypeGlAccount> PaymentMethodTypeGlAccounts { get; set; }
    public ICollection<PaymentMethodType> PaymentMethodTypes { get; set; }
    public ICollection<PaymentMethod> PaymentMethods { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<ProductCategoryGlAccount> ProductCategoryGlAccounts { get; set; }
    public ICollection<ProductGlAccount> ProductGlAccounts { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
    public ICollection<TaxAuthorityGlAccount> TaxAuthorityGlAccounts { get; set; }
    public ICollection<VarianceReasonGlAccount> VarianceReasonGlAccounts { get; set; }
}