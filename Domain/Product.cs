using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Product
{
    public Product()
    {
        AgreementProductAppls = new HashSet<AgreementProductAppl>();
        Agreements = new HashSet<Agreement>();
        CartAbandonedLines = new HashSet<CartAbandonedLine>();
        CommunicationEventProducts = new HashSet<CommunicationEventProduct>();
        CostComponents = new HashSet<CostComponent>();
        CustRequestItems = new HashSet<CustRequestItem>();
        FixedAssetProducts = new HashSet<FixedAssetProduct>();
        FixedAssets = new HashSet<FixedAsset>();
        GoodIdentifications = new HashSet<GoodIdentification>();
        InventoryItemTempRes = new HashSet<InventoryItemTempRe>();
        InventoryItems = new HashSet<InventoryItem>();
        InvoiceItems = new HashSet<InvoiceItem>();
        MrpEvents = new HashSet<MrpEvent>();
        OrderItems = new HashSet<OrderItem>();
        OrderSummaryEntries = new HashSet<OrderSummaryEntry>();
        PartyNeeds = new HashSet<PartyNeed>();
        ProductAssocProductIdToNavigations = new HashSet<ProductAssoc>();
        ProductAssocProducts = new HashSet<ProductAssoc>();
        ProductAttributes = new HashSet<ProductAttribute>();
        ProductAverageCosts = new HashSet<ProductAverageCost>();
        ProductCategoryMembers = new HashSet<ProductCategoryMember>();
        ProductConfigProducts = new HashSet<ProductConfigProduct>();
        ProductConfigStats = new HashSet<ProductConfigStat>();
        ProductConfigs = new HashSet<ProductConfig>();
        ProductContents = new HashSet<ProductContent>();
        ProductCostComponentCalcs = new HashSet<ProductCostComponentCalc>();
        ProductFacilities = new HashSet<ProductFacility>();
        ProductFacilityAssocs = new HashSet<ProductFacilityAssoc>();
        ProductFacilityLocations = new HashSet<ProductFacilityLocation>();
        ProductFeatureApplAttrs = new HashSet<ProductFeatureApplAttr>();
        ProductFeatureAppls = new HashSet<ProductFeatureAppl>();
        ProductGeos = new HashSet<ProductGeo>();
        ProductGlAccounts = new HashSet<ProductGlAccount>();
        ProductGroupOrders = new HashSet<ProductGroupOrder>();
        ProductKeywordNews = new HashSet<ProductKeywordNew>();
        ProductMaints = new HashSet<ProductMaint>();
        ProductManufacturingRuleProductIdForNavigations = new HashSet<ProductManufacturingRule>();
        ProductManufacturingRuleProductIdInNavigations = new HashSet<ProductManufacturingRule>();
        ProductManufacturingRuleProductIdInSubstNavigations = new HashSet<ProductManufacturingRule>();
        ProductManufacturingRuleProducts = new HashSet<ProductManufacturingRule>();
        ProductMeters = new HashSet<ProductMeter>();
        ProductOrderItems = new HashSet<ProductOrderItem>();
        ProductPaymentMethodTypes = new HashSet<ProductPaymentMethodType>();
        ProductPrices = new HashSet<ProductPrice>();
        ProductPromoProducts = new HashSet<ProductPromoProduct>();
        ProductReviews = new HashSet<ProductReview>();
        ProductRoles = new HashSet<ProductRole>();
        ProductSubscriptionResources = new HashSet<ProductSubscriptionResource>();
        QuoteItems = new HashSet<QuoteItem>();
        ReorderGuidelines = new HashSet<ReorderGuideline>();
        Requirements = new HashSet<Requirement>();
        ReturnItems = new HashSet<ReturnItem>();
        SalesForecastDetails = new HashSet<SalesForecastDetail>();
        ShipmentItems = new HashSet<ShipmentItem>();
        ShipmentPackageContents = new HashSet<ShipmentPackageContent>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
        ShoppingListItems = new HashSet<ShoppingListItem>();
        Subscriptions = new HashSet<Subscription>();
        SupplierProducts = new HashSet<SupplierProduct>();
        VendorProducts = new HashSet<VendorProduct>();
        WorkEffortGoodStandards = new HashSet<WorkEffortGoodStandard>();
    }

    public string ProductId { get; set; } = null!;
    public string? ProductTypeId { get; set; }
    public string? PrimaryProductCategoryId { get; set; }
    public string? FacilityId { get; set; }
    public DateTime? IntroductionDate { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? SupportDiscontinuationDate { get; set; }
    public DateTime? SalesDiscontinuationDate { get; set; }
    public string? SalesDiscWhenNotAvail { get; set; }
    public string? InternalName { get; set; }
    public string? BrandName { get; set; }
    public string? Comments { get; set; }
    public string? ProductName { get; set; }
    public string? ProductNameArabic { get; set; }
    public string? ProductNameTurkish { get; set; }
    public string? Description { get; set; }
    public string? LongDescription { get; set; }
    public string? PriceDetailText { get; set; }
    public string? SmallImageUrl { get; set; }
    public string? MediumImageUrl { get; set; }
    public string? LargeImageUrl { get; set; }
    public string? DetailImageUrl { get; set; }
    public string? OriginalImageUrl { get; set; }
    public string? DetailScreen { get; set; }
    public string? InventoryMessage { get; set; }
    public string? InventoryItemTypeId { get; set; }
    public string? RequireInventory { get; set; }
    public string? QuantityUomId { get; set; }
    public decimal? QuantityIncluded { get; set; }
    public int? PiecesIncluded { get; set; }
    public int? ServiceLifeDays { get; set; }
    public int? ServiceLifeMileage { get; set; }
    public string? RequireAmount { get; set; }
    public decimal? FixedAmount { get; set; }
    public string? AmountUomTypeId { get; set; }
    public string? WeightUomId { get; set; }
    public decimal? ShippingWeight { get; set; }
    public decimal? ProductWeight { get; set; }
    public string? HeightUomId { get; set; }
    public decimal? ProductHeight { get; set; }
    public decimal? ShippingHeight { get; set; }
    public string? WidthUomId { get; set; }
    public decimal? ProductWidth { get; set; }
    public decimal? ShippingWidth { get; set; }
    public string? DepthUomId { get; set; }
    public decimal? ProductDepth { get; set; }
    public decimal? ShippingDepth { get; set; }
    public string? DiameterUomId { get; set; }
    public decimal? ProductDiameter { get; set; }
    public decimal? ProductRating { get; set; }
    public string? RatingTypeEnum { get; set; }
    public string? Returnable { get; set; }
    public string? Taxable { get; set; }
    public string? ChargeShipping { get; set; }
    public string? AutoCreateKeywords { get; set; }
    public string? IncludeInPromotions { get; set; }
    public string? IsVirtual { get; set; }
    public string? IsVariant { get; set; }
    public string? VirtualVariantMethodEnum { get; set; }
    public string? OriginGeoId { get; set; }
    public string? RequirementMethodEnumId { get; set; }
    public int? BillOfMaterialLevel { get; set; }
    public decimal? ReservMaxPersons { get; set; }
    public decimal? Reserv2ndPPPerc { get; set; }
    public decimal? ReservNthPPPerc { get; set; }
    public string? ConfigId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? InShippingBox { get; set; }
    public string? DefaultShipmentBoxTypeId { get; set; }
    public string? LotIdFilledIn { get; set; }
    public string? OrderDecimalQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UomType? AmountUomType { get; set; }
    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public ShipmentBoxType? DefaultShipmentBoxType { get; set; }
    public Uom? DepthUom { get; set; }
    public Uom? DiameterUom { get; set; }
    public Facility? Facility { get; set; }
    public Uom? HeightUom { get; set; }
    public InventoryItemType? InventoryItemType { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public Geo? OriginGeo { get; set; }
    public ProductCategory? PrimaryProductCategory { get; set; }
    public ProductType? ProductType { get; set; }
    public Uom? QuantityUom { get; set; }
    public Enumeration? RatingTypeEnumNavigation { get; set; }
    public Enumeration? RequirementMethodEnum { get; set; }
    public Enumeration? VirtualVariantMethodEnumNavigation { get; set; }
    public Uom? WeightUom { get; set; }
    public Uom? WidthUom { get; set; }
    public ProductCalculatedInfo? ProductCalculatedInfo { get; set; }
    public ICollection<AgreementProductAppl> AgreementProductAppls { get; set; }
    public ICollection<Agreement> Agreements { get; set; }
    public ICollection<CartAbandonedLine> CartAbandonedLines { get; set; }
    public ICollection<CommunicationEventProduct> CommunicationEventProducts { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<CustRequestItem> CustRequestItems { get; set; }
    public ICollection<FixedAssetProduct> FixedAssetProducts { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GoodIdentification> GoodIdentifications { get; set; }
    public ICollection<InventoryItemTempRe> InventoryItemTempRes { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<MrpEvent> MrpEvents { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<OrderSummaryEntry> OrderSummaryEntries { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<ProductAssoc> ProductAssocProductIdToNavigations { get; set; }
    public ICollection<ProductAssoc> ProductAssocProducts { get; set; }
    public ICollection<ProductAttribute> ProductAttributes { get; set; }
    public ICollection<ProductAverageCost> ProductAverageCosts { get; set; }
    public ICollection<ProductCategoryMember> ProductCategoryMembers { get; set; }
    public ICollection<ProductConfigProduct> ProductConfigProducts { get; set; }
    public ICollection<ProductConfigStat> ProductConfigStats { get; set; }
    public ICollection<ProductConfig> ProductConfigs { get; set; }
    public ICollection<ProductContent> ProductContents { get; set; }
    public ICollection<ProductCostComponentCalc> ProductCostComponentCalcs { get; set; }
    public ICollection<ProductFacility> ProductFacilities { get; set; }
    public ICollection<ProductFacilityAssoc> ProductFacilityAssocs { get; set; }
    public ICollection<ProductFacilityLocation> ProductFacilityLocations { get; set; }
    public ICollection<ProductFeatureApplAttr> ProductFeatureApplAttrs { get; set; }
    public ICollection<ProductFeatureAppl> ProductFeatureAppls { get; set; }
    public ICollection<ProductGeo> ProductGeos { get; set; }
    public ICollection<ProductGlAccount> ProductGlAccounts { get; set; }
    public ICollection<ProductGroupOrder> ProductGroupOrders { get; set; }
    public ICollection<ProductKeywordNew> ProductKeywordNews { get; set; }
    public ICollection<ProductMaint> ProductMaints { get; set; }
    public ICollection<ProductManufacturingRule> ProductManufacturingRuleProductIdForNavigations { get; set; }
    public ICollection<ProductManufacturingRule> ProductManufacturingRuleProductIdInNavigations { get; set; }
    public ICollection<ProductManufacturingRule> ProductManufacturingRuleProductIdInSubstNavigations { get; set; }
    public ICollection<ProductManufacturingRule> ProductManufacturingRuleProducts { get; set; }
    public ICollection<ProductMeter> ProductMeters { get; set; }
    public ICollection<ProductOrderItem> ProductOrderItems { get; set; }
    public ICollection<ProductPaymentMethodType> ProductPaymentMethodTypes { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
    public ICollection<ProductPromoProduct> ProductPromoProducts { get; set; }
    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductRole> ProductRoles { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResources { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<ReorderGuideline> ReorderGuidelines { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
    public ICollection<SalesForecastDetail> SalesForecastDetails { get; set; }
    public ICollection<ShipmentItem> ShipmentItems { get; set; }
    public ICollection<ShipmentPackageContent> ShipmentPackageContents { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
    public ICollection<ShoppingListItem> ShoppingListItems { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
    public ICollection<SupplierProduct> SupplierProducts { get; set; }
    public ICollection<VendorProduct> VendorProducts { get; set; }
    public ICollection<WorkEffortGoodStandard> WorkEffortGoodStandards { get; set; }
    public virtual ICollection<ServiceSpecification> ServiceSpecifications { get; set; }
}