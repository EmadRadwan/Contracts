using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductPromo
{
    public ProductPromo()
    {
        AgreementPromoAppls = new HashSet<AgreementPromoAppl>();
        MarketingCampaignPromos = new HashSet<MarketingCampaignPromo>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        ProductPromoActions = new HashSet<ProductPromoAction>();
        ProductPromoCategories = new HashSet<ProductPromoCategory>();
        ProductPromoCodes = new HashSet<ProductPromoCode>();
        ProductPromoConds = new HashSet<ProductPromoCond>();
        ProductPromoContents = new HashSet<ProductPromoContent>();
        ProductPromoProducts = new HashSet<ProductPromoProduct>();
        ProductPromoRules = new HashSet<ProductPromoRule>();
        ProductPromoUses = new HashSet<ProductPromoUse>();
        ProductStorePromoAppls = new HashSet<ProductStorePromoAppl>();
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
    }

    public string ProductPromoId { get; set; } = null!;
    public string? PromoName { get; set; }
    public string? PromoText { get; set; }
    public string? UserEntered { get; set; }
    public string? ShowToCustomer { get; set; }
    public string? RequireCode { get; set; }
    public int? UseLimitPerOrder { get; set; }
    public int? UseLimitPerCustomer { get; set; }
    public int? UseLimitPerPromotion { get; set; }
    public decimal? BillbackFactor { get; set; }
    public string? OverrideOrgPartyId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public Party? OverrideOrgParty { get; set; }
    public ICollection<AgreementPromoAppl> AgreementPromoAppls { get; set; }
    public ICollection<MarketingCampaignPromo> MarketingCampaignPromos { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<ProductPromoAction> ProductPromoActions { get; set; }
    public ICollection<ProductPromoCategory> ProductPromoCategories { get; set; }
    public ICollection<ProductPromoCode> ProductPromoCodes { get; set; }
    public ICollection<ProductPromoCond> ProductPromoConds { get; set; }
    public ICollection<ProductPromoContent> ProductPromoContents { get; set; }
    public ICollection<ProductPromoProduct> ProductPromoProducts { get; set; }
    public ICollection<ProductPromoRule> ProductPromoRules { get; set; }
    public ICollection<ProductPromoUse> ProductPromoUses { get; set; }
    public ICollection<ProductStorePromoAppl> ProductStorePromoAppls { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
}