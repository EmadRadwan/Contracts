using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Enumeration
{
    public Enumeration()
    {
        AllocationPlanItems = new HashSet<AllocationPlanItem>();
        CommunicationEvents = new HashSet<CommunicationEvent>();
        ContentPurposeOperations = new HashSet<ContentPurposeOperation>();
        Contents = new HashSet<Content>();
        CustRequests = new HashSet<CustRequest>();
        EmailTemplateSettings = new HashSet<EmailTemplateSetting>();
        FacilityLocations = new HashSet<FacilityLocation>();
        FinAccountTrans = new HashSet<FinAccountTran>();
        FinAccountTypes = new HashSet<FinAccountType>();
        FixedAssets = new HashSet<FixedAsset>();
        GeoPoints = new HashSet<GeoPoint>();
        GiftCardFulfillments = new HashSet<GiftCardFulfillment>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        JobInterviews = new HashSet<JobInterview>();
        JobManagerLocks = new HashSet<JobManagerLock>();
        JobRequisitionExamTypeEnums = new HashSet<JobRequisition>();
        JobRequisitionJobPostingTypeEnums = new HashSet<JobRequisition>();
        KeywordThesaurus = new HashSet<KeywordThesauru>();
        OrderHeaders = new HashSet<OrderHeader>();
        OrderItemChangeChangeTypeEnums = new HashSet<OrderItemChange>();
        OrderItemChangeReasonEnums = new HashSet<OrderItemChange>();
        OrderNotifications = new HashSet<OrderNotification>();
        PartyAcctgPreferenceCogsMethods = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceInvoiceSequenceEnums = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceOrderSequenceEnums = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceQuoteSequenceEnums = new HashSet<PartyAcctgPreference>();
        PartyAcctgPreferenceTaxForms = new HashSet<PartyAcctgPreference>();
        PaymentGatewayResponsePaymentServiceTypeEnums = new HashSet<PaymentGatewayResponse>();
        PaymentGatewayResponseTransCodeEnums = new HashSet<PaymentGatewayResponse>();
        PersonEmploymentStatusEnums = new HashSet<Person>();
        PersonMaritalStatusEnums = new HashSet<Person>();
        PersonResidenceStatusEnums = new HashSet<Person>();
        ProductCategoryLinks = new HashSet<ProductCategoryLink>();
        ProductFacilityReplenishMethodEnums = new HashSet<ProductFacility>();
        ProductFacilityRequirementMethodEnums = new HashSet<ProductFacility>();
        ProductGeos = new HashSet<ProductGeo>();
        ProductKeywordNews = new HashSet<ProductKeywordNew>();
        ProductPriceCondInputParamEnums = new HashSet<ProductPriceCond>();
        ProductPriceCondOperatorEnums = new HashSet<ProductPriceCond>();
        ProductPromoActions = new HashSet<ProductPromoAction>();
        ProductPromoCategories = new HashSet<ProductPromoCategory>();
        ProductPromoCondInputParamEnums = new HashSet<ProductPromoCond>();
        ProductPromoCondOperatorEnums = new HashSet<ProductPromoCond>();
        ProductPromoProducts = new HashSet<ProductPromoProduct>();
        ProductRatingTypeEnumNavigations = new HashSet<Product>();
        ProductRequirementMethodEnums = new HashSet<Product>();
        ProductStoreDefaultSalesChannelEnums = new HashSet<ProductStore>();
        ProductStoreEmailSettings = new HashSet<ProductStoreEmailSetting>();
        ProductStoreFinActSettings = new HashSet<ProductStoreFinActSetting>();
        ProductStoreKeywordOvrds = new HashSet<ProductStoreKeywordOvrd>();
        ProductStorePaymentSettings = new HashSet<ProductStorePaymentSetting>();
        ProductStoreRequirementMethodEnums = new HashSet<ProductStore>();
        ProductStoreReserveOrderEnums = new HashSet<ProductStore>();
        ProductStoreStoreCreditAccountEnums = new HashSet<ProductStore>();
        ProductStoreTelecomSettings = new HashSet<ProductStoreTelecomSetting>();
        ProductStoreVendorPayments = new HashSet<ProductStoreVendorPayment>();
        ProductVirtualVariantMethodEnumNavigations = new HashSet<Product>();
        Quotes = new HashSet<Quote>();
        SalesOpportunities = new HashSet<SalesOpportunity>();
        TrackingCodeVisits = new HashSet<TrackingCodeVisit>();
        UomConversionDateds = new HashSet<UomConversionDated>();
        VisualThemeResources = new HashSet<VisualThemeResource>();
        WorkEffortPartyAssignmentDelegateReasonEnums = new HashSet<WorkEffortPartyAssignment>();
        WorkEffortPartyAssignmentExpectationEnums = new HashSet<WorkEffortPartyAssignment>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string EnumId { get; set; } = null!;
    public string? EnumTypeId { get; set; }
    public string? EnumCode { get; set; }
    public string? SequenceId { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EnumerationType? EnumType { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItems { get; set; }
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<ContentPurposeOperation> ContentPurposeOperations { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<EmailTemplateSetting> EmailTemplateSettings { get; set; }
    public ICollection<FacilityLocation> FacilityLocations { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<FinAccountType> FinAccountTypes { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GeoPoint> GeoPoints { get; set; }
    public ICollection<GiftCardFulfillment> GiftCardFulfillments { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<JobInterview> JobInterviews { get; set; }
    public ICollection<JobManagerLock> JobManagerLocks { get; set; }
    public ICollection<JobRequisition> JobRequisitionExamTypeEnums { get; set; }
    public ICollection<JobRequisition> JobRequisitionJobPostingTypeEnums { get; set; }
    public ICollection<KeywordThesauru> KeywordThesaurus { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<OrderItemChange> OrderItemChangeChangeTypeEnums { get; set; }
    public ICollection<OrderItemChange> OrderItemChangeReasonEnums { get; set; }
    public ICollection<OrderNotification> OrderNotifications { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceCogsMethods { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceInvoiceSequenceEnums { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceOrderSequenceEnums { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceQuoteSequenceEnums { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferenceTaxForms { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponsePaymentServiceTypeEnums { get; set; }
    public ICollection<PaymentGatewayResponse> PaymentGatewayResponseTransCodeEnums { get; set; }
    public ICollection<Person> PersonEmploymentStatusEnums { get; set; }
    public ICollection<Person> PersonMaritalStatusEnums { get; set; }
    public ICollection<Person> PersonResidenceStatusEnums { get; set; }
    public ICollection<ProductCategoryLink> ProductCategoryLinks { get; set; }
    public ICollection<ProductFacility> ProductFacilityReplenishMethodEnums { get; set; }
    public ICollection<ProductFacility> ProductFacilityRequirementMethodEnums { get; set; }
    public ICollection<ProductGeo> ProductGeos { get; set; }
    public ICollection<ProductKeywordNew> ProductKeywordNews { get; set; }
    public ICollection<ProductPriceCond> ProductPriceCondInputParamEnums { get; set; }
    public ICollection<ProductPriceCond> ProductPriceCondOperatorEnums { get; set; }
    public ICollection<ProductPromoAction> ProductPromoActions { get; set; }
    public ICollection<ProductPromoCategory> ProductPromoCategories { get; set; }
    public ICollection<ProductPromoCond> ProductPromoCondInputParamEnums { get; set; }
    public ICollection<ProductPromoCond> ProductPromoCondOperatorEnums { get; set; }
    public ICollection<ProductPromoProduct> ProductPromoProducts { get; set; }
    public ICollection<Product> ProductRatingTypeEnumNavigations { get; set; }
    public ICollection<Product> ProductRequirementMethodEnums { get; set; }
    public ICollection<ProductStore> ProductStoreDefaultSalesChannelEnums { get; set; }
    public ICollection<ProductStoreEmailSetting> ProductStoreEmailSettings { get; set; }
    public ICollection<ProductStoreFinActSetting> ProductStoreFinActSettings { get; set; }
    public ICollection<ProductStoreKeywordOvrd> ProductStoreKeywordOvrds { get; set; }
    public ICollection<ProductStorePaymentSetting> ProductStorePaymentSettings { get; set; }
    public ICollection<ProductStore> ProductStoreRequirementMethodEnums { get; set; }
    public ICollection<ProductStore> ProductStoreReserveOrderEnums { get; set; }
    public ICollection<ProductStore> ProductStoreStoreCreditAccountEnums { get; set; }
    public ICollection<ProductStoreTelecomSetting> ProductStoreTelecomSettings { get; set; }
    public ICollection<ProductStoreVendorPayment> ProductStoreVendorPayments { get; set; }
    public ICollection<Product> ProductVirtualVariantMethodEnumNavigations { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
    public ICollection<TrackingCodeVisit> TrackingCodeVisits { get; set; }
    public ICollection<UomConversionDated> UomConversionDateds { get; set; }
    public ICollection<VisualThemeResource> VisualThemeResources { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignmentDelegateReasonEnums { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignmentExpectationEnums { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}