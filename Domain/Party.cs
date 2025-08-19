using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class Party
{
    public Party()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        AgreementPartyApplics = new HashSet<AgreementPartyApplic>();
        AgreementRoles = new HashSet<AgreementRole>();
        BillingAccountRoles = new HashSet<BillingAccountRole>();
        BudgetReviews = new HashSet<BudgetReview>();
        BudgetRoles = new HashSet<BudgetRole>();
        CarrierShipmentBoxTypes = new HashSet<CarrierShipmentBoxType>();
        CarrierShipmentMethods = new HashSet<CarrierShipmentMethod>();
        CommunicationEventPartyIdFromNavigations = new HashSet<CommunicationEvent>();
        CommunicationEventPartyIdToNavigations = new HashSet<CommunicationEvent>();
        CommunicationEventRoles = new HashSet<CommunicationEventRole>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        ContactListParties = new HashSet<ContactListParty>();
        ContactLists = new HashSet<ContactList>();
        ContentApprovals = new HashSet<ContentApproval>();
        ContentRevisions = new HashSet<ContentRevision>();
        CostComponents = new HashSet<CostComponent>();
        CustRequestParties = new HashSet<CustRequestParty>();
        CustRequestTypes = new HashSet<CustRequestType>();
        CustRequests = new HashSet<CustRequest>();
        CustomTimePeriods = new HashSet<CustomTimePeriod>();
        EmplLeaveApproverParties = new HashSet<EmplLeave>();
        EmplLeaveParties = new HashSet<EmplLeave>();
        EmplPositionFulfillments = new HashSet<EmplPositionFulfillment>();
        EmplPositions = new HashSet<EmplPosition>();
        EmploymentApps = new HashSet<EmploymentApp>();
        EmploymentPartyIdFromNavigations = new HashSet<Employment>();
        EmploymentPartyIdToNavigations = new HashSet<Employment>();
        Facilities = new HashSet<Facility>();
        FacilityCarrierShipments = new HashSet<FacilityCarrierShipment>();
        FacilityParties = new HashSet<FacilityParty>();
        FinAccountOrganizationParties = new HashSet<FinAccount>();
        FinAccountOwnerParties = new HashSet<FinAccount>();
        FinAccountTranParties = new HashSet<FinAccountTran>();
        FinAccountTranPerformedByParties = new HashSet<FinAccountTran>();
        FinAccountTypeGlAccounts = new HashSet<FinAccountTypeGlAccount>();
        FixedAssetRegistrations = new HashSet<FixedAssetRegistration>();
        FixedAssetTypeGlAccounts = new HashSet<FixedAssetTypeGlAccount>();
        FixedAssets = new HashSet<FixedAsset>();
        GiftCardFulfillments = new HashSet<GiftCardFulfillment>();
        GlAccountHistories = new HashSet<GlAccountHistory>();
        GlAccountOrganizations = new HashSet<GlAccountOrganization>();
        GlAccountTypeDefaults = new HashSet<GlAccountTypeDefault>();
        GlJournals = new HashSet<GlJournal>();
        GlReconciliations = new HashSet<GlReconciliation>();
        InventoryItemOwnerParties = new HashSet<InventoryItem>();
        InventoryItemParties = new HashSet<InventoryItem>();
        InvoiceItemOverrideOrgParties = new HashSet<InvoiceItem>();
        InvoiceItemTaxAuthParties = new HashSet<InvoiceItem>();
        InvoiceItemTypeGlAccounts = new HashSet<InvoiceItemTypeGlAccount>();
        InvoiceParties = new HashSet<Invoice>();
        InvoicePartyIdFromNavigations = new HashSet<Invoice>();
        InvoiceRoles = new HashSet<InvoiceRole>();
        ItemIssuanceRoles = new HashSet<ItemIssuanceRole>();
        JobInterviewJobIntervieweeParties = new HashSet<JobInterview>();
        JobInterviewJobInterviewerParties = new HashSet<JobInterview>();
        NoteData = new HashSet<NoteDatum>();
        OrderItemRoles = new HashSet<OrderItemRole>();
        OrderItemShipGroupCarrierParties = new HashSet<OrderItemShipGroup>();
        OrderItemShipGroupSupplierParties = new HashSet<OrderItemShipGroup>();
        OrderItemShipGroupVendorParties = new HashSet<OrderItemShipGroup>();
        OrderRoles = new HashSet<OrderRole>();
        PartyAttributes = new HashSet<PartyAttribute>();
        PartyBenefitPartyIdFromNavigations = new HashSet<PartyBenefit>();
        PartyBenefitPartyIdToNavigations = new HashSet<PartyBenefit>();
        PartyCarrierAccountCarrierParties = new HashSet<PartyCarrierAccount>();
        PartyCarrierAccountParties = new HashSet<PartyCarrierAccount>();
        PartyClassifications = new HashSet<PartyClassification>();
        PartyContactMechPurposes = new HashSet<PartyContactMechPurpose>();
        PartyContactMeches = new HashSet<PartyContactMech>();
        PartyContents = new HashSet<PartyContent>();
        PartyDataSources = new HashSet<PartyDataSource>();
        PartyGeoPoints = new HashSet<PartyGeoPoint>();
        PartyGlAccountOrganizationParties = new HashSet<PartyGlAccount>();
        PartyGlAccountParties = new HashSet<PartyGlAccount>();
        PartyIdentifications = new HashSet<PartyIdentification>();
        PartyInvitationGroupAssocs = new HashSet<PartyInvitationGroupAssoc>();
        PartyInvitations = new HashSet<PartyInvitation>();
        PartyNameHistories = new HashSet<PartyNameHistory>();
        PartyNeeds = new HashSet<PartyNeed>();
        PartyNotes = new HashSet<PartyNote>();
        PartyPrefDocTypeTpls = new HashSet<PartyPrefDocTypeTpl>();
        PartyProfileDefaults = new HashSet<PartyProfileDefault>();
        PartyQuals = new HashSet<PartyQual>();
        PartyRateNews = new HashSet<PartyRateNew>();
        PartyResumes = new HashSet<PartyResume>();
        PartyRoles = new HashSet<PartyRole>();
        PartySkills = new HashSet<PartySkill>();
        PartyStatuses = new HashSet<PartyStatus>();
        PartyTaxAuthInfos = new HashSet<PartyTaxAuthInfo>();
        PaymentGlAccountTypeMaps = new HashSet<PaymentGlAccountTypeMap>();
        PaymentMethodTypeGlAccounts = new HashSet<PaymentMethodTypeGlAccount>();
        PaymentMethods = new HashSet<PaymentMethod>();
        PaymentPartyIdFromNavigations = new HashSet<Payment>();
        PaymentPartyIdToNavigations = new HashSet<Payment>();
        PayrollPreferences = new HashSet<PayrollPreference>();
        PerfReviewEmployeeParties = new HashSet<PerfReview>();
        PerfReviewItems = new HashSet<PerfReviewItem>();
        PerfReviewManagerParties = new HashSet<PerfReview>();
        PerformanceNotes = new HashSet<PerformanceNote>();
        PersonTrainings = new HashSet<PersonTraining>();
        ProductAverageCosts = new HashSet<ProductAverageCost>();
        ProductCategoryGlAccounts = new HashSet<ProductCategoryGlAccount>();
        ProductGlAccounts = new HashSet<ProductGlAccount>();
        ProductPrices = new HashSet<ProductPrice>();
        ProductPromoCodeParties = new HashSet<ProductPromoCodeParty>();
        ProductPromoUses = new HashSet<ProductPromoUse>();
        ProductPromos = new HashSet<ProductPromo>();
        ProductStoreVendorPayments = new HashSet<ProductStoreVendorPayment>();
        ProductStoreVendorShipmentCarrierParties = new HashSet<ProductStoreVendorShipment>();
        ProductStoreVendorShipmentVendorParties = new HashSet<ProductStoreVendorShipment>();
        ProductStores = new HashSet<ProductStore>();
        QuoteRoles = new HashSet<QuoteRole>();
        Quotes = new HashSet<Quote>();
        RateAmounts = new HashSet<RateAmount>();
        ReorderGuidelines = new HashSet<ReorderGuideline>();
        RequirementRoles = new HashSet<RequirementRole>();
        RespondingParties = new HashSet<RespondingParty>();
        ReturnHeaderFromParties = new HashSet<ReturnHeader>();
        ReturnHeaderToParties = new HashSet<ReturnHeader>();
        SalesForecastHistoryInternalParties = new HashSet<SalesForecastHistory>();
        SalesForecastHistoryOrganizationParties = new HashSet<SalesForecastHistory>();
        SalesForecastInternalParties = new HashSet<SalesForecast>();
        SalesForecastOrganizationParties = new HashSet<SalesForecast>();
        SalesOpportunityRoles = new HashSet<SalesOpportunityRole>();
        ShipmentCostEstimates = new HashSet<ShipmentCostEstimate>();
        ShipmentPartyIdFromNavigations = new HashSet<Shipment>();
        ShipmentPartyIdToNavigations = new HashSet<Shipment>();
        ShipmentReceiptRoles = new HashSet<ShipmentReceiptRole>();
        ShipmentRouteSegments = new HashSet<ShipmentRouteSegment>();
        ShoppingLists = new HashSet<ShoppingList>();
        SubscriptionOriginatedFromParties = new HashSet<Subscription>();
        SubscriptionParties = new HashSet<Subscription>();
        SupplierProductFeatures = new HashSet<SupplierProductFeature>();
        SupplierProducts = new HashSet<SupplierProduct>();
        TaxAuthorities = new HashSet<TaxAuthority>();
        TaxAuthorityGlAccounts = new HashSet<TaxAuthorityGlAccount>();
        TimeEntries = new HashSet<TimeEntry>();
        TimesheetClientParties = new HashSet<Timesheet>();
        TimesheetParties = new HashSet<Timesheet>();
        TimesheetRoles = new HashSet<TimesheetRole>();
        UserLoginHistories = new HashSet<UserLoginHistory>();
        UserLogins = new HashSet<UserLogin>();
        VarianceReasonGlAccounts = new HashSet<VarianceReasonGlAccount>();
        VendorProducts = new HashSet<VendorProduct>();
        WebUserPreferences = new HashSet<WebUserPreference>();
        WorkEffortEventReminders = new HashSet<WorkEffortEventReminder>();
    }

    public string PartyId { get; set; } = null!;
    public string? PartyTypeId { get; set; }
    public string? MainRole { get; set; }
    public string? ExternalId { get; set; }
    public string? PreferredCurrencyUomId { get; set; }
    public string? Description { get; set; }
    public string? StatusId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? DataSourceId { get; set; }
    public string? IsUnread { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public DataSource? DataSource { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public PartyType? PartyType { get; set; }
    public Uom? PreferredCurrencyUom { get; set; }
    public StatusItem? Status { get; set; }
    public Affiliate Affiliate { get; set; } = null!;
    public PartyAcctgPreference PartyAcctgPreference { get; set; } = null!;
    public PartyGroup PartyGroup { get; set; } = null!;
    public PartyIcsAvsOverride PartyIcsAvsOverride { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<AgreementPartyApplic> AgreementPartyApplics { get; set; }
    public ICollection<AgreementRole> AgreementRoles { get; set; }
    public ICollection<BillingAccountRole> BillingAccountRoles { get; set; }
    public ICollection<BudgetReview> BudgetReviews { get; set; }
    public ICollection<BudgetRole> BudgetRoles { get; set; }
    public ICollection<CarrierShipmentBoxType> CarrierShipmentBoxTypes { get; set; }
    public ICollection<CarrierShipmentMethod> CarrierShipmentMethods { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventPartyIdFromNavigations { get; set; }
    public ICollection<CommunicationEvent> CommunicationEventPartyIdToNavigations { get; set; }
    public ICollection<CommunicationEventRole> CommunicationEventRoles { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<ContactListParty> ContactListParties { get; set; }
    public ICollection<ContactList> ContactLists { get; set; }
    public ICollection<ContentApproval> ContentApprovals { get; set; }
    public ICollection<ContentRevision> ContentRevisions { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<CustRequestParty> CustRequestParties { get; set; }
    public ICollection<CustRequestType> CustRequestTypes { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<CustomTimePeriod> CustomTimePeriods { get; set; }
    public ICollection<EmplLeave> EmplLeaveApproverParties { get; set; }
    public ICollection<EmplLeave> EmplLeaveParties { get; set; }
    public ICollection<EmplPositionFulfillment> EmplPositionFulfillments { get; set; }
    public ICollection<EmplPosition> EmplPositions { get; set; }
    public ICollection<EmploymentApp> EmploymentApps { get; set; }
    public ICollection<Employment> EmploymentPartyIdFromNavigations { get; set; }
    public ICollection<Employment> EmploymentPartyIdToNavigations { get; set; }
    public ICollection<Facility> Facilities { get; set; }
    public ICollection<FacilityCarrierShipment> FacilityCarrierShipments { get; set; }
    public ICollection<FacilityParty> FacilityParties { get; set; }
    public ICollection<FinAccount> FinAccountOrganizationParties { get; set; }
    public ICollection<FinAccount> FinAccountOwnerParties { get; set; }
    public ICollection<FinAccountTran> FinAccountTranParties { get; set; }
    public ICollection<FinAccountTran> FinAccountTranPerformedByParties { get; set; }
    public ICollection<FinAccountTypeGlAccount> FinAccountTypeGlAccounts { get; set; }
    public ICollection<FixedAssetRegistration> FixedAssetRegistrations { get; set; }
    public ICollection<FixedAssetTypeGlAccount> FixedAssetTypeGlAccounts { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<GiftCardFulfillment> GiftCardFulfillments { get; set; }
    public ICollection<GlAccountHistory> GlAccountHistories { get; set; }
    public ICollection<GlAccountOrganization> GlAccountOrganizations { get; set; }
    public ICollection<GlAccountTypeDefault> GlAccountTypeDefaults { get; set; }
    public ICollection<GlJournal> GlJournals { get; set; }
    public ICollection<GlReconciliation> GlReconciliations { get; set; }
    public ICollection<InventoryItem> InventoryItemOwnerParties { get; set; }
    public ICollection<InventoryItem> InventoryItemParties { get; set; }
    public ICollection<InvoiceItem> InvoiceItemOverrideOrgParties { get; set; }
    public ICollection<InvoiceItem> InvoiceItemTaxAuthParties { get; set; }
    public ICollection<InvoiceItemTypeGlAccount> InvoiceItemTypeGlAccounts { get; set; }
    public ICollection<Invoice> InvoiceParties { get; set; }
    public ICollection<Invoice> InvoicePartyIdFromNavigations { get; set; }
    public ICollection<InvoiceRole> InvoiceRoles { get; set; }
    public ICollection<ItemIssuanceRole> ItemIssuanceRoles { get; set; }
    public ICollection<JobInterview> JobInterviewJobIntervieweeParties { get; set; }
    public ICollection<JobInterview> JobInterviewJobInterviewerParties { get; set; }
    public ICollection<NoteDatum> NoteData { get; set; }
    public ICollection<OrderItemRole> OrderItemRoles { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroupCarrierParties { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroupSupplierParties { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroupVendorParties { get; set; }
    public ICollection<OrderRole> OrderRoles { get; set; }
    public ICollection<PartyAttribute> PartyAttributes { get; set; }
    public ICollection<PartyBenefit> PartyBenefitPartyIdFromNavigations { get; set; }
    public ICollection<PartyBenefit> PartyBenefitPartyIdToNavigations { get; set; }
    public ICollection<PartyCarrierAccount> PartyCarrierAccountCarrierParties { get; set; }
    public ICollection<PartyCarrierAccount> PartyCarrierAccountParties { get; set; }
    public ICollection<PartyClassification> PartyClassifications { get; set; }
    public ICollection<PartyContactMechPurpose> PartyContactMechPurposes { get; set; }
    public ICollection<PartyContactMech> PartyContactMeches { get; set; }
    public ICollection<PartyContent> PartyContents { get; set; }
    public ICollection<PartyDataSource> PartyDataSources { get; set; }
    public ICollection<PartyGeoPoint> PartyGeoPoints { get; set; }
    public ICollection<PartyGlAccount> PartyGlAccountOrganizationParties { get; set; }
    public ICollection<PartyGlAccount> PartyGlAccountParties { get; set; }
    public ICollection<PartyIdentification> PartyIdentifications { get; set; }
    public ICollection<PartyInvitationGroupAssoc> PartyInvitationGroupAssocs { get; set; }
    public ICollection<PartyInvitation> PartyInvitations { get; set; }
    public ICollection<PartyNameHistory> PartyNameHistories { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<PartyNote> PartyNotes { get; set; }
    public ICollection<PartyPrefDocTypeTpl> PartyPrefDocTypeTpls { get; set; }
    public ICollection<PartyProfileDefault> PartyProfileDefaults { get; set; }
    public ICollection<PartyQual> PartyQuals { get; set; }
    public ICollection<PartyRateNew> PartyRateNews { get; set; }
    public ICollection<PartyResume> PartyResumes { get; set; }
    public ICollection<PartyRole> PartyRoles { get; set; }
    public ICollection<PartySkill> PartySkills { get; set; }
    public ICollection<PartyStatus> PartyStatuses { get; set; }
    public ICollection<PartyTaxAuthInfo> PartyTaxAuthInfos { get; set; }
    public ICollection<PaymentGlAccountTypeMap> PaymentGlAccountTypeMaps { get; set; }
    public ICollection<PaymentMethodTypeGlAccount> PaymentMethodTypeGlAccounts { get; set; }
    public ICollection<PaymentMethod> PaymentMethods { get; set; }
    public ICollection<Payment> PaymentPartyIdFromNavigations { get; set; }
    public ICollection<Payment> PaymentPartyIdToNavigations { get; set; }
    public ICollection<PayrollPreference> PayrollPreferences { get; set; }
    public ICollection<PerfReview> PerfReviewEmployeeParties { get; set; }
    public ICollection<PerfReviewItem> PerfReviewItems { get; set; }
    public ICollection<PerfReview> PerfReviewManagerParties { get; set; }
    public ICollection<PerformanceNote> PerformanceNotes { get; set; }
    public ICollection<PersonTraining> PersonTrainings { get; set; }
    public ICollection<ProductAverageCost> ProductAverageCosts { get; set; }
    public ICollection<ProductCategoryGlAccount> ProductCategoryGlAccounts { get; set; }
    public ICollection<ProductGlAccount> ProductGlAccounts { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
    public ICollection<ProductPromoCodeParty> ProductPromoCodeParties { get; set; }
    public ICollection<ProductPromoUse> ProductPromoUses { get; set; }
    public ICollection<ProductPromo> ProductPromos { get; set; }
    public ICollection<ProductStoreVendorPayment> ProductStoreVendorPayments { get; set; }
    public ICollection<ProductStoreVendorShipment> ProductStoreVendorShipmentCarrierParties { get; set; }
    public ICollection<ProductStoreVendorShipment> ProductStoreVendorShipmentVendorParties { get; set; }
    public ICollection<ProductStore> ProductStores { get; set; }
    public ICollection<QuoteRole> QuoteRoles { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
    public ICollection<ReorderGuideline> ReorderGuidelines { get; set; }
    public ICollection<RequirementRole> RequirementRoles { get; set; }
    public ICollection<RespondingParty> RespondingParties { get; set; }
    public ICollection<ReturnHeader> ReturnHeaderFromParties { get; set; }
    public ICollection<ReturnHeader> ReturnHeaderToParties { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistoryInternalParties { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistoryOrganizationParties { get; set; }
    public ICollection<SalesForecast> SalesForecastInternalParties { get; set; }
    public ICollection<SalesForecast> SalesForecastOrganizationParties { get; set; }
    public ICollection<SalesOpportunityRole> SalesOpportunityRoles { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimates { get; set; }
    public ICollection<Shipment> ShipmentPartyIdFromNavigations { get; set; }
    public ICollection<Shipment> ShipmentPartyIdToNavigations { get; set; }
    public ICollection<ShipmentReceiptRole> ShipmentReceiptRoles { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegments { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
    public ICollection<Subscription> SubscriptionOriginatedFromParties { get; set; }
    public ICollection<Subscription> SubscriptionParties { get; set; }
    public ICollection<SupplierProductFeature> SupplierProductFeatures { get; set; }
    public ICollection<SupplierProduct> SupplierProducts { get; set; }
    public ICollection<TaxAuthority> TaxAuthorities { get; set; }
    public ICollection<TaxAuthorityGlAccount> TaxAuthorityGlAccounts { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; }
    public ICollection<Timesheet> TimesheetClientParties { get; set; }
    public ICollection<Timesheet> TimesheetParties { get; set; }
    public ICollection<TimesheetRole> TimesheetRoles { get; set; }
    public ICollection<UserLoginHistory> UserLoginHistories { get; set; }
    public ICollection<UserLogin> UserLogins { get; set; }
    public ICollection<VarianceReasonGlAccount> VarianceReasonGlAccounts { get; set; }
    public ICollection<VendorProduct> VendorProducts { get; set; }
    public ICollection<WebUserPreference> WebUserPreferences { get; set; }
    public ICollection<WorkEffortEventReminder> WorkEffortEventReminders { get; set; }
    public ICollection<AppUserLogin> AppUserLogins { get; set; }
    public virtual ICollection<Vehicle> Vehicles { get; set; }
}