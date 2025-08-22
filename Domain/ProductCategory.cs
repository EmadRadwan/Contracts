using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductCategory
{
    public ProductCategory()
    {
        InversePrimaryParentCategory = new HashSet<ProductCategory>();
        MarketInterests = new HashSet<MarketInterest>();
        PartyNeeds = new HashSet<PartyNeed>();
        ProdCatalogCategories = new HashSet<ProdCatalogCategory>();
        ProductCategoryAttributes = new HashSet<ProductCategoryAttribute>();
        ProductCategoryContents = new HashSet<ProductCategoryContent>();
        ProductCategoryGlAccounts = new HashSet<ProductCategoryGlAccount>();
        ProductCategoryLinks = new HashSet<ProductCategoryLink>();
        ProductCategoryMembers = new HashSet<ProductCategoryMember>();
        ProductCategoryRoles = new HashSet<ProductCategoryRole>();
        ProductCategoryRollupParentProductCategories = new HashSet<ProductCategoryRollup>();
        ProductCategoryRollupProductCategories = new HashSet<ProductCategoryRollup>();
        ProductFeatureCatGrpAppls = new HashSet<ProductFeatureCatGrpAppl>();
        ProductFeatureCategoryAppls = new HashSet<ProductFeatureCategoryAppl>();
        ProductPromoCategories = new HashSet<ProductPromoCategory>();
        Products = new HashSet<Product>();
        SalesForecastDetails = new HashSet<SalesForecastDetail>();
        Subscriptions = new HashSet<Subscription>();
        TaxAuthorityCategories = new HashSet<TaxAuthorityCategory>();
        TaxAuthorityRateProducts = new HashSet<TaxAuthorityRateProduct>();
    }

    public string ProductCategoryId { get; set; } = null!;
    public string? ProductCategoryTypeId { get; set; }
    public string? PrimaryParentCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public string? LongDescription { get; set; }
    public string? CategoryImageUrl { get; set; }
    public string? LinkOneImageUrl { get; set; }
    public string? LinkTwoImageUrl { get; set; }
    public string? DetailScreen { get; set; }
    public string? ShowInSelect { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory? PrimaryParentCategory { get; set; }
    public ProductCategoryType? ProductCategoryType { get; set; }
    public ICollection<ProductCategory> InversePrimaryParentCategory { get; set; }
    public ICollection<MarketInterest> MarketInterests { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<ProdCatalogCategory> ProdCatalogCategories { get; set; }
    public ICollection<ProductCategoryAttribute> ProductCategoryAttributes { get; set; }
    public ICollection<ProductCategoryContent> ProductCategoryContents { get; set; }
    public ICollection<ProductCategoryGlAccount> ProductCategoryGlAccounts { get; set; }
    public ICollection<ProductCategoryLink> ProductCategoryLinks { get; set; }
    public ICollection<ProductCategoryMember> ProductCategoryMembers { get; set; }
    public ICollection<ProductCategoryRole> ProductCategoryRoles { get; set; }
    public ICollection<ProductCategoryRollup> ProductCategoryRollupParentProductCategories { get; set; }
    public ICollection<ProductCategoryRollup> ProductCategoryRollupProductCategories { get; set; }
    public ICollection<ProductFeatureCatGrpAppl> ProductFeatureCatGrpAppls { get; set; }
    public ICollection<ProductFeatureCategoryAppl> ProductFeatureCategoryAppls { get; set; }
    public ICollection<ProductPromoCategory> ProductPromoCategories { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<SalesForecastDetail> SalesForecastDetails { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<TaxAuthorityCategory> TaxAuthorityCategories { get; set; }
    public ICollection<TaxAuthorityRateProduct> TaxAuthorityRateProducts { get; set; }
    public virtual ICollection<ServiceSpecification> ServiceSpecificationsAsMake { get; set; }
    public virtual ICollection<ServiceSpecification> ServiceSpecificationsAsModel { get; set; }
    public virtual ICollection<ServiceRate> ServiceRatesAsMake { get; set; }
    public virtual ICollection<ServiceRate> ServiceRatesAsModel { get; set; }
}