using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TaxAuthority
{
    public TaxAuthority()
    {
        OrderAdjustments = new HashSet<OrderAdjustment>();
        PartyTaxAuthInfos = new HashSet<PartyTaxAuthInfo>();
        ProductStores = new HashSet<ProductStore>();
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
        TaxAuthorityAssocTaxAuths = new HashSet<TaxAuthorityAssoc>();
        TaxAuthorityAssocToTaxAuths = new HashSet<TaxAuthorityAssoc>();
        TaxAuthorityCategories = new HashSet<TaxAuthorityCategory>();
        TaxAuthorityGlAccounts = new HashSet<TaxAuthorityGlAccount>();
        TaxAuthorityRateProducts = new HashSet<TaxAuthorityRateProduct>();
    }

    public string TaxAuthGeoId { get; set; } = null!;
    public string TaxAuthPartyId { get; set; } = null!;
    public string? RequireTaxIdForExemption { get; set; }
    public string? TaxIdFormatPattern { get; set; }
    public string? IncludeTaxInPrice { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo TaxAuthGeo { get; set; } = null!;
    public Party TaxAuthParty { get; set; } = null!;
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<PartyTaxAuthInfo> PartyTaxAuthInfos { get; set; }
    public ICollection<ProductStore> ProductStores { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
    public ICollection<TaxAuthorityAssoc> TaxAuthorityAssocTaxAuths { get; set; }
    public ICollection<TaxAuthorityAssoc> TaxAuthorityAssocToTaxAuths { get; set; }
    public ICollection<TaxAuthorityCategory> TaxAuthorityCategories { get; set; }
    public ICollection<TaxAuthorityGlAccount> TaxAuthorityGlAccounts { get; set; }
    public ICollection<TaxAuthorityRateProduct> TaxAuthorityRateProducts { get; set; }
}