using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccountType
{
    public GlAccountType()
    {
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        CostComponentCalcCostGlAccountTypes = new HashSet<CostComponentCalc>();
        CostComponentCalcOffsettingGlAccountTypes = new HashSet<CostComponentCalc>();
        GlAccountTypeDefaults = new HashSet<GlAccountTypeDefault>();
        GlAccounts = new HashSet<GlAccount>();
        InverseParentType = new HashSet<GlAccountType>();
        PartyGlAccounts = new HashSet<PartyGlAccount>();
        PaymentGlAccountTypeMaps = new HashSet<PaymentGlAccountTypeMap>();
        ProductCategoryGlAccounts = new HashSet<ProductCategoryGlAccount>();
        ProductGlAccounts = new HashSet<ProductGlAccount>();
    }

    public string GlAccountTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountType? ParentType { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<CostComponentCalc> CostComponentCalcCostGlAccountTypes { get; set; }
    public ICollection<CostComponentCalc> CostComponentCalcOffsettingGlAccountTypes { get; set; }
    public ICollection<GlAccountTypeDefault> GlAccountTypeDefaults { get; set; }
    public ICollection<GlAccount> GlAccounts { get; set; }
    public ICollection<GlAccountType> InverseParentType { get; set; }
    public ICollection<PartyGlAccount> PartyGlAccounts { get; set; }
    public ICollection<PaymentGlAccountTypeMap> PaymentGlAccountTypeMaps { get; set; }
    public ICollection<ProductCategoryGlAccount> ProductCategoryGlAccounts { get; set; }
    public ICollection<ProductGlAccount> ProductGlAccounts { get; set; }
}