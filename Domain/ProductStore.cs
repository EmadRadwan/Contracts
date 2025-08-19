using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductStore
{
    public ProductStore()
    {
        CustRequests = new HashSet<CustRequest>();
        InventoryItemTempRes = new HashSet<InventoryItemTempRe>();
        OrderHeaders = new HashSet<OrderHeader>();
        PartyProfileDefaults = new HashSet<PartyProfileDefault>();
        ProductReviews = new HashSet<ProductReview>();
        ProductStoreCatalogs = new HashSet<ProductStoreCatalog>();
        ProductStoreEmailSettings = new HashSet<ProductStoreEmailSetting>();
        ProductStoreFacilities = new HashSet<ProductStoreFacility>();
        ProductStoreFinActSettings = new HashSet<ProductStoreFinActSetting>();
        ProductStoreGroupMembers = new HashSet<ProductStoreGroupMember>();
        ProductStoreKeywordOvrds = new HashSet<ProductStoreKeywordOvrd>();
        ProductStorePaymentSettings = new HashSet<ProductStorePaymentSetting>();
        ProductStorePromoAppls = new HashSet<ProductStorePromoAppl>();
        ProductStoreRoles = new HashSet<ProductStoreRole>();
        ProductStoreSurveyAppls = new HashSet<ProductStoreSurveyAppl>();
        ProductStoreTelecomSettings = new HashSet<ProductStoreTelecomSetting>();
        ProductStoreVendorPayments = new HashSet<ProductStoreVendorPayment>();
        ProductStoreVendorShipments = new HashSet<ProductStoreVendorShipment>();
        Quotes = new HashSet<Quote>();
        SegmentGroups = new HashSet<SegmentGroup>();
        ShoppingLists = new HashSet<ShoppingList>();
        TaxAuthorityRateProducts = new HashSet<TaxAuthorityRateProduct>();
        WebSites = new HashSet<WebSite>();
    }

    public string ProductStoreId { get; set; } = null!;
    public string? PrimaryStoreGroupId { get; set; }
    public string? StoreName { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? PayToPartyId { get; set; }
    public int? DaysToCancelNonPay { get; set; }
    public string? ManualAuthIsCapture { get; set; }
    public string? ProrateShipping { get; set; }
    public string? ProrateTaxes { get; set; }
    public string? ViewCartOnAdd { get; set; }
    public string? AutoSaveCart { get; set; }
    public string? AutoApproveReviews { get; set; }
    public string? IsDemoStore { get; set; }
    public string? IsImmediatelyFulfilled { get; set; }
    public string? InventoryFacilityId { get; set; }
    public string? OneInventoryFacility { get; set; }
    public string? CheckInventory { get; set; }
    public string? ReserveInventory { get; set; }
    public string? ReserveOrderEnumId { get; set; }
    public string? RequireInventory { get; set; }
    public string? BalanceResOnOrderCreation { get; set; }
    public string? RequirementMethodEnumId { get; set; }
    public string? SOrderNumberPrefix { get; set; }
    public string? POrderNumberPrefix { get; set; }
    public string? DefaultLocaleString { get; set; }
    public string? DefaultCurrencyUomId { get; set; }
    public string? DefaultTimeZoneString { get; set; }
    public string? DefaultSalesChannelEnumId { get; set; }
    public string? AllowPassword { get; set; }
    public string? DefaultPassword { get; set; }
    public string? ExplodeOrderItems { get; set; }
    public string? CheckGcBalance { get; set; }
    public string? RetryFailedAuths { get; set; }
    public string? HeaderApprovedStatus { get; set; }
    public string? ItemApprovedStatus { get; set; }
    public string? DigitalItemApprovedStatus { get; set; }
    public string? HeaderDeclinedStatus { get; set; }
    public string? ItemDeclinedStatus { get; set; }
    public string? HeaderCancelStatus { get; set; }
    public string? ItemCancelStatus { get; set; }
    public string? AuthDeclinedMessage { get; set; }
    public string? AuthFraudMessage { get; set; }
    public string? AuthErrorMessage { get; set; }
    public string? VisualThemeId { get; set; }
    public string? StoreCreditAccountEnumId { get; set; }
    public string? UsePrimaryEmailUsername { get; set; }
    public string? RequireCustomerRole { get; set; }
    public string? AutoInvoiceDigitalItems { get; set; }
    public string? ReqShipAddrForDigItems { get; set; }
    public string? ShowCheckoutGiftOptions { get; set; }
    public string? SelectPaymentTypePerItem { get; set; }
    public string? ShowPricesWithVatTax { get; set; }
    public string? ShowTaxIsExempt { get; set; }
    public string? TaxExempt { get; set; }
    public string? VatTaxAuthGeoId { get; set; }
    public string? VatTaxAuthPartyId { get; set; }
    public string? EnableAutoSuggestionList { get; set; }
    public string? EnableDigProdUpload { get; set; }
    public string? ProdSearchExcludeVariants { get; set; }
    public string? DigProdUploadCategoryId { get; set; }
    public string? AutoOrderCcTryExp { get; set; }
    public string? AutoOrderCcTryOtherCards { get; set; }
    public string? AutoOrderCcTryLaterNsf { get; set; }
    public int? AutoOrderCcTryLaterMax { get; set; }
    public int? StoreCreditValidDays { get; set; }
    public string? AutoApproveInvoice { get; set; }
    public string? AutoApproveOrder { get; set; }
    public string? ShipIfCaptureFails { get; set; }
    public string? SetOwnerUponIssuance { get; set; }
    public string? ReqReturnInventoryReceive { get; set; }
    public string? AddToCartRemoveIncompat { get; set; }
    public string? AddToCartReplaceUpsell { get; set; }
    public string? SplitPayPrefPerShpGrp { get; set; }
    public string? ManagedByLot { get; set; }
    public string? ShowOutOfStockProducts { get; set; }
    public string? OrderDecimalQuantity { get; set; }
    public string? AllowComment { get; set; }
    public string? AllocateInventory { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? DefaultCurrencyUom { get; set; }
    public Enumeration? DefaultSalesChannelEnum { get; set; }
    public StatusItem? DigitalItemApprovedStatusNavigation { get; set; }
    public StatusItem? HeaderApprovedStatusNavigation { get; set; }
    public StatusItem? HeaderCancelStatusNavigation { get; set; }
    public StatusItem? HeaderDeclinedStatusNavigation { get; set; }
    public Facility? InventoryFacility { get; set; }
    public StatusItem? ItemApprovedStatusNavigation { get; set; }
    public StatusItem? ItemCancelStatusNavigation { get; set; }
    public StatusItem? ItemDeclinedStatusNavigation { get; set; }
    public Party? PayToParty { get; set; }
    public ProductStoreGroup? PrimaryStoreGroup { get; set; }
    public Enumeration? RequirementMethodEnum { get; set; }
    public Enumeration? ReserveOrderEnum { get; set; }
    public Enumeration? StoreCreditAccountEnum { get; set; }
    public TaxAuthority? VatTaxAuth { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<InventoryItemTempRe> InventoryItemTempRes { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<PartyProfileDefault> PartyProfileDefaults { get; set; }
    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductStoreCatalog> ProductStoreCatalogs { get; set; }
    public ICollection<ProductStoreEmailSetting> ProductStoreEmailSettings { get; set; }
    public ICollection<ProductStoreFacility> ProductStoreFacilities { get; set; }
    public ICollection<ProductStoreFinActSetting> ProductStoreFinActSettings { get; set; }
    public ICollection<ProductStoreGroupMember> ProductStoreGroupMembers { get; set; }
    public ICollection<ProductStoreKeywordOvrd> ProductStoreKeywordOvrds { get; set; }
    public ICollection<ProductStorePaymentSetting> ProductStorePaymentSettings { get; set; }
    public ICollection<ProductStorePromoAppl> ProductStorePromoAppls { get; set; }
    public ICollection<ProductStoreRole> ProductStoreRoles { get; set; }
    public ICollection<ProductStoreSurveyAppl> ProductStoreSurveyAppls { get; set; }
    public ICollection<ProductStoreTelecomSetting> ProductStoreTelecomSettings { get; set; }
    public ICollection<ProductStoreVendorPayment> ProductStoreVendorPayments { get; set; }
    public ICollection<ProductStoreVendorShipment> ProductStoreVendorShipments { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<SegmentGroup> SegmentGroups { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
    public ICollection<TaxAuthorityRateProduct> TaxAuthorityRateProducts { get; set; }
    public ICollection<WebSite> WebSites { get; set; }
    public ICollection<ServiceRate> ServiceRates { get; set; }
}