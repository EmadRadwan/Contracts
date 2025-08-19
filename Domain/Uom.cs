using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Uom
{
    public Uom()
    {
        AcctgTransEntryCurrencyUoms = new HashSet<AcctgTransEntry>();
        AcctgTransEntryOrigCurrencyUoms = new HashSet<AcctgTransEntry>();
        BillingAccountTerms = new HashSet<BillingAccountTerm>();
        BillingAccounts = new HashSet<BillingAccount>();
        CostComponentCalcs = new HashSet<CostComponentCalc>();
        CostComponents = new HashSet<CostComponent>();
        CustRequestCurrencyUoms = new HashSet<CustRequest>();
        CustRequestMaximumAmountUoms = new HashSet<CustRequest>();
        FacilityDefaultDimensionUoms = new HashSet<Facility>();
        FacilityDefaultWeightUoms = new HashSet<Facility>();
        FacilityFacilitySizeUoms = new HashSet<Facility>();
        FinAccounts = new HashSet<FinAccount>();
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        FixedAssetProducts = new HashSet<FixedAssetProduct>();
        FixedAssetStdCosts = new HashSet<FixedAssetStdCost>();
        FixedAssets = new HashSet<FixedAsset>();
        GeoPoints = new HashSet<GeoPoint>();
        InventoryItemCurrencyUoms = new HashSet<InventoryItem>();
        InventoryItemUoms = new HashSet<InventoryItem>();
        InvoiceItems = new HashSet<InvoiceItem>();
        Invoices = new HashSet<Invoice>();
        MarketingCampaigns = new HashSet<MarketingCampaign>();
        OrderDeliveryScheduleTotalCubicUoms = new HashSet<OrderDeliverySchedule>();
        OrderDeliveryScheduleTotalWeightUoms = new HashSet<OrderDeliverySchedule>();
        OrderHeaders = new HashSet<OrderHeader>();
        OrderItems = new HashSet<OrderItem>();
        OrderTerms = new HashSet<OrderTerm>();
        Parties = new HashSet<Party>();
        PartyAcctgPreferences = new HashSet<PartyAcctgPreference>();
        PaymentActualCurrencyUoms = new HashSet<Payment>();
        PaymentCurrencyUoms = new HashSet<Payment>();
        PaymentGatewayResponses = new HashSet<PaymentGatewayResponse>();
        PeriodTypes = new HashSet<PeriodType>();
        ProductContents = new HashSet<ProductContent>();
        ProductDepthUoms = new HashSet<Product>();
        ProductDiameterUoms = new HashSet<Product>();
        ProductFeaturePrices = new HashSet<ProductFeaturePrice>();
        ProductFeatures = new HashSet<ProductFeature>();
        ProductHeightUoms = new HashSet<Product>();
        ProductMaints = new HashSet<ProductMaint>();
        ProductMeterTypes = new HashSet<ProductMeterType>();
        ProductMeters = new HashSet<ProductMeter>();
        ProductPriceCurrencyUoms = new HashSet<ProductPrice>();
        ProductPriceTermUoms = new HashSet<ProductPrice>();
        ProductQuantityUoms = new HashSet<Product>();
        ProductStores = new HashSet<ProductStore>();
        ProductSubscriptionResourceAvailableTimeUoms = new HashSet<ProductSubscriptionResource>();
        ProductSubscriptionResourceCanclAutmExtTimeUoms = new HashSet<ProductSubscriptionResource>();
        ProductSubscriptionResourceGracePeriodOnExpiryUoms = new HashSet<ProductSubscriptionResource>();
        ProductSubscriptionResourceMaxLifeTimeUoms = new HashSet<ProductSubscriptionResource>();
        ProductSubscriptionResourceUseTimeUoms = new HashSet<ProductSubscriptionResource>();
        ProductWeightUoms = new HashSet<Product>();
        ProductWidthUoms = new HashSet<Product>();
        QuoteItems = new HashSet<QuoteItem>();
        Quotes = new HashSet<Quote>();
        RateAmounts = new HashSet<RateAmount>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        SalesForecastDetails = new HashSet<SalesForecastDetail>();
        SalesForecastHistories = new HashSet<SalesForecastHistory>();
        SalesForecasts = new HashSet<SalesForecast>();
        SalesOpportunities = new HashSet<SalesOpportunity>();
        SalesOpportunityHistories = new HashSet<SalesOpportunityHistory>();
        ShipmentBoxTypeDimensionUoms = new HashSet<ShipmentBoxType>();
        ShipmentBoxTypeWeightUoms = new HashSet<ShipmentBoxType>();
        ShipmentCostEstimatePriceUoms = new HashSet<ShipmentCostEstimate>();
        ShipmentCostEstimateQuantityUoms = new HashSet<ShipmentCostEstimate>();
        ShipmentCostEstimateWeightUoms = new HashSet<ShipmentCostEstimate>();
        ShipmentPackageDimensionUoms = new HashSet<ShipmentPackage>();
        ShipmentPackageRouteSegs = new HashSet<ShipmentPackageRouteSeg>();
        ShipmentPackageWeightUoms = new HashSet<ShipmentPackage>();
        ShipmentRouteSegmentBillingWeightUoms = new HashSet<ShipmentRouteSegment>();
        ShipmentRouteSegmentCurrencyUoms = new HashSet<ShipmentRouteSegment>();
        ShipmentTimeEstimates = new HashSet<ShipmentTimeEstimate>();
        Shipments = new HashSet<Shipment>();
        SubscriptionAvailableTimeUoms = new HashSet<Subscription>();
        SubscriptionCanclAutmExtTimeUoms = new HashSet<Subscription>();
        SubscriptionGracePeriodOnExpiryUoms = new HashSet<Subscription>();
        SubscriptionMaxLifeTimeUoms = new HashSet<Subscription>();
        SubscriptionUseTimeUoms = new HashSet<Subscription>();
        SupplierProductCurrencyUoms = new HashSet<SupplierProduct>();
        SupplierProductFeatures = new HashSet<SupplierProductFeature>();
        SupplierProductQuantityUoms = new HashSet<SupplierProduct>();
        UomConversionDatedUomIdToNavigations = new HashSet<UomConversionDated>();
        UomConversionDatedUoms = new HashSet<UomConversionDated>();
        UomConversionUomIdToNavigations = new HashSet<UomConversion>();
        UomConversionUoms = new HashSet<UomConversion>();
        UomGroups = new HashSet<UomGroup>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string UomId { get; set; } = null!;
    public string? UomTypeId { get; set; }
    public string? Abbreviation { get; set; }
    public int? NumericCode { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UomType? UomType { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntryCurrencyUoms { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntryOrigCurrencyUoms { get; set; }
    public ICollection<BillingAccountTerm> BillingAccountTerms { get; set; }
    public ICollection<BillingAccount> BillingAccounts { get; set; }
    public ICollection<CostComponentCalc> CostComponentCalcs { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<CustRequest> CustRequestCurrencyUoms { get; set; }
    public ICollection<CustRequest> CustRequestMaximumAmountUoms { get; set; }
    public ICollection<Facility> FacilityDefaultDimensionUoms { get; set; }
    public ICollection<Facility> FacilityDefaultWeightUoms { get; set; }
    public ICollection<Facility> FacilityFacilitySizeUoms { get; set; }
    public ICollection<FinAccount> FinAccounts { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<FixedAssetProduct> FixedAssetProducts { get; set; }
    public ICollection<FixedAssetStdCost> FixedAssetStdCosts { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GeoPoint> GeoPoints { get; set; }
    public ICollection<InventoryItem> InventoryItemCurrencyUoms { get; set; }
    public ICollection<InventoryItem> InventoryItemUoms { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<MarketingCampaign> MarketingCampaigns { get; set; }
    public ICollection<OrderDeliverySchedule> OrderDeliveryScheduleTotalCubicUoms { get; set; }
    public ICollection<OrderDeliverySchedule> OrderDeliveryScheduleTotalWeightUoms { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<OrderTerm> OrderTerms { get; set; }
    public ICollection<Party> Parties { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferences { get; set; }
    public ICollection<Payment> PaymentActualCurrencyUoms { get; set; }
    public ICollection<Payment> PaymentCurrencyUoms { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponses { get; set; }
    public ICollection<PeriodType> PeriodTypes { get; set; }
    public ICollection<ProductContent> ProductContents { get; set; }
    public ICollection<Product> ProductDepthUoms { get; set; }
    public ICollection<Product> ProductDiameterUoms { get; set; }
    public ICollection<ProductFeaturePrice> ProductFeaturePrices { get; set; }
    public ICollection<ProductFeature> ProductFeatures { get; set; }
    public ICollection<Product> ProductHeightUoms { get; set; }
    public ICollection<ProductMaint> ProductMaints { get; set; }
    public ICollection<ProductMeterType> ProductMeterTypes { get; set; }
    public ICollection<ProductMeter> ProductMeters { get; set; }
    public ICollection<ProductPrice> ProductPriceCurrencyUoms { get; set; }
    public ICollection<ProductPrice> ProductPriceTermUoms { get; set; }
    public ICollection<Product> ProductQuantityUoms { get; set; }
    public ICollection<ProductStore> ProductStores { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResourceAvailableTimeUoms { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResourceCanclAutmExtTimeUoms { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResourceGracePeriodOnExpiryUoms { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResourceMaxLifeTimeUoms { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResourceUseTimeUoms { get; set; }
    public ICollection<Product> ProductWeightUoms { get; set; }
    public ICollection<Product> ProductWidthUoms { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<SalesForecastDetail> SalesForecastDetails { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistories { get; set; }
    public ICollection<SalesForecast> SalesForecasts { get; set; }
    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
    public ICollection<SalesOpportunityHistory> SalesOpportunityHistories { get; set; }
    public ICollection<ShipmentBoxType> ShipmentBoxTypeDimensionUoms { get; set; }
    public ICollection<ShipmentBoxType> ShipmentBoxTypeWeightUoms { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimatePriceUoms { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateQuantityUoms { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateWeightUoms { get; set; }
    public ICollection<ShipmentPackage> ShipmentPackageDimensionUoms { get; set; }
    public ICollection<ShipmentPackageRouteSeg> ShipmentPackageRouteSegs { get; set; }
    public ICollection<ShipmentPackage> ShipmentPackageWeightUoms { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentBillingWeightUoms { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentCurrencyUoms { get; set; }
    public ICollection<ShipmentTimeEstimate> ShipmentTimeEstimates { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
    public ICollection<Subscription> SubscriptionAvailableTimeUoms { get; set; }
    public ICollection<Subscription> SubscriptionCanclAutmExtTimeUoms { get; set; }
    public ICollection<Subscription> SubscriptionGracePeriodOnExpiryUoms { get; set; }
    public ICollection<Subscription> SubscriptionMaxLifeTimeUoms { get; set; }
    public ICollection<Subscription> SubscriptionUseTimeUoms { get; set; }
    public ICollection<SupplierProduct> SupplierProductCurrencyUoms { get; set; }
    public ICollection<SupplierProductFeature> SupplierProductFeatures { get; set; }
    public ICollection<SupplierProduct> SupplierProductQuantityUoms { get; set; }
    public ICollection<UomConversionDated> UomConversionDatedUomIdToNavigations { get; set; }
    public ICollection<UomConversionDated> UomConversionDatedUoms { get; set; }
    public ICollection<UomConversion> UomConversionUomIdToNavigations { get; set; }
    public ICollection<UomConversion> UomConversionUoms { get; set; }
    public ICollection<UomGroup> UomGroups { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}