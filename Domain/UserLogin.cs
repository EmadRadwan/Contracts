namespace Domain;

public class UserLogin
{
    public UserLogin()
    {
        AgreementStatuses = new HashSet<AgreementStatus>();
        AllocationPlanHeaderCreatedByUserLoginNavigations = new HashSet<AllocationPlanHeader>();
        AllocationPlanHeaderLastModifiedByUserLoginNavigations = new HashSet<AllocationPlanHeader>();
        AllocationPlanItemCreatedByUserLoginNavigations = new HashSet<AllocationPlanItem>();
        AllocationPlanItemLastModifiedByUserLoginNavigations = new HashSet<AllocationPlanItem>();
        BudgetStatuses = new HashSet<BudgetStatus>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        ContactListCreatedByUserLoginNavigations = new HashSet<ContactList>();
        ContactListLastModifiedByUserLoginNavigations = new HashSet<ContactList>();
        ContentAssocCreatedByUserLoginNavigations = new HashSet<ContentAssoc>();
        ContentAssocLastModifiedByUserLoginNavigations = new HashSet<ContentAssoc>();
        ContentCreatedByUserLoginNavigations = new HashSet<Content>();
        ContentLastModifiedByUserLoginNavigations = new HashSet<Content>();
        CustRequestStatuses = new HashSet<CustRequestStatus>();
        DataResourceCreatedByUserLoginNavigations = new HashSet<DataResource>();
        DataResourceLastModifiedByUserLoginNavigations = new HashSet<DataResource>();
        FinAccountStatuses = new HashSet<FinAccountStatus>();
        InventoryItemStatuses = new HashSet<InventoryItemStatus>();
        InvoiceStatuses = new HashSet<InvoiceStatus>();
        ItemIssuances = new HashSet<ItemIssuance>();
        JobSandboxAuthUserLogins = new HashSet<JobSandbox>();
        JobSandboxRunAsUserNavigations = new HashSet<JobSandbox>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        OrderHeaders = new HashSet<OrderHeader>();
        OrderItemChangeByUserLogins = new HashSet<OrderItem>();
        OrderItemChanges = new HashSet<OrderItemChange>();
        OrderItemDontCancelSetUserLoginNavigations = new HashSet<OrderItem>();
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        OrderStatuses = new HashSet<OrderStatus>();
        PartyCreatedByUserLoginNavigations = new HashSet<Party>();
        PartyLastModifiedByUserLoginNavigations = new HashSet<Party>();
        PartyStatuses = new HashSet<PartyStatus>();
        PicklistRoleCreatedByUserLoginNavigations = new HashSet<PicklistRole>();
        PicklistRoleLastModifiedByUserLoginNavigations = new HashSet<PicklistRole>();
        PicklistStatusHistories = new HashSet<PicklistStatusHistory>();
        PicklistStatuses = new HashSet<PicklistStatus>();
        ProductCreatedByUserLoginNavigations = new HashSet<Product>();
        ProductFeaturePriceCreatedByUserLoginNavigations = new HashSet<ProductFeaturePrice>();
        ProductFeaturePriceLastModifiedByUserLoginNavigations = new HashSet<ProductFeaturePrice>();
        ProductLastModifiedByUserLoginNavigations = new HashSet<Product>();
        ProductPriceChanges = new HashSet<ProductPriceChange>();
        ProductPriceCreatedByUserLoginNavigations = new HashSet<ProductPrice>();
        ProductPriceLastModifiedByUserLoginNavigations = new HashSet<ProductPrice>();
        ProductPromoCodeCreatedByUserLoginNavigations = new HashSet<ProductPromoCode>();
        ProductPromoCodeLastModifiedByUserLoginNavigations = new HashSet<ProductPromoCode>();
        ProductPromoCreatedByUserLoginNavigations = new HashSet<ProductPromo>();
        ProductPromoLastModifiedByUserLoginNavigations = new HashSet<ProductPromo>();
        ProductReviews = new HashSet<ProductReview>();
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
        RequirementStatuses = new HashSet<RequirementStatus>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
        ReturnStatuses = new HashSet<ReturnStatus>();
        SalesForecastCreatedByUserLogins = new HashSet<SalesForecast>();
        SalesForecastHistories = new HashSet<SalesForecastHistory>();
        SalesForecastModifiedByUserLogins = new HashSet<SalesForecast>();
        SalesOpportunities = new HashSet<SalesOpportunity>();
        SalesOpportunityHistories = new HashSet<SalesOpportunityHistory>();
        ShipmentReceipts = new HashSet<ShipmentReceipt>();
        ShipmentStatuses = new HashSet<ShipmentStatus>();
        TestingStatuses = new HashSet<TestingStatus>();
        Timesheets = new HashSet<Timesheet>();
        UserLoginHistories = new HashSet<UserLoginHistory>();
        UserLoginPasswordHistories = new HashSet<UserLoginPasswordHistory>();
        UserLoginSecurityGroups = new HashSet<UserLoginSecurityGroup>();
        WebUserPreferences = new HashSet<WebUserPreference>();
        WorkEffortPartyAssignments = new HashSet<WorkEffortPartyAssignment>();
        WorkEffortReviews = new HashSet<WorkEffortReview>();
        WorkEffortStatuses = new HashSet<WorkEffortStatus>();
    }

    public string UserLoginId { get; set; } = null!;
    public string? CurrentPassword { get; set; }
    public string? PasswordHint { get; set; }
    public string? IsSystem { get; set; }
    public string? Enabled { get; set; }
    public string? HasLoggedOut { get; set; }
    public string? RequirePasswordChange { get; set; }
    public string? LastCurrencyUom { get; set; }
    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }
    public DateTime? DisabledDateTime { get; set; }
    public int? SuccessiveFailedLogins { get; set; }
    public string? ExternalAuthId { get; set; }
    public string? UserLdapDn { get; set; }
    public string? DisabledBy { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? PartyId { get; set; }

    public Party? Party { get; set; }
    public UserLoginSession UserLoginSession { get; set; } = null!;
    public ICollection<AgreementStatus> AgreementStatuses { get; set; }
    public ICollection<AllocationPlanHeader> AllocationPlanHeaderCreatedByUserLoginNavigations { get; set; }
    public ICollection<AllocationPlanHeader> AllocationPlanHeaderLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItemCreatedByUserLoginNavigations { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItemLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<BudgetStatus> BudgetStatuses { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<ContactList> ContactListCreatedByUserLoginNavigations { get; set; }
    public ICollection<ContactList> ContactListLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<ContentAssoc> ContentAssocCreatedByUserLoginNavigations { get; set; }
    public ICollection<ContentAssoc> ContentAssocLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<Content> ContentCreatedByUserLoginNavigations { get; set; }
    public ICollection<Content> ContentLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<CustRequestStatus> CustRequestStatuses { get; set; }
    public ICollection<DataResource> DataResourceCreatedByUserLoginNavigations { get; set; }
    public ICollection<DataResource> DataResourceLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<FinAccountStatus> FinAccountStatuses { get; set; }
    public ICollection<InventoryItemStatus> InventoryItemStatuses { get; set; }
    public ICollection<InvoiceStatus> InvoiceStatuses { get; set; }
    public ICollection<ItemIssuance> ItemIssuances { get; set; }
    public ICollection<JobSandbox> JobSandboxAuthUserLogins { get; set; }
    public ICollection<JobSandbox> JobSandboxRunAsUserNavigations { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<OrderItem> OrderItemChangeByUserLogins { get; set; }
    public ICollection<OrderItemChange> OrderItemChanges { get; set; }
    public ICollection<OrderItem> OrderItemDontCancelSetUserLoginNavigations { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<OrderStatus> OrderStatuses { get; set; }
    public ICollection<Party> PartyCreatedByUserLoginNavigations { get; set; }
    public ICollection<Party> PartyLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<PartyStatus> PartyStatuses { get; set; }
    public ICollection<PicklistRole> PicklistRoleCreatedByUserLoginNavigations { get; set; }
    public ICollection<PicklistRole> PicklistRoleLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<PicklistStatusHistory> PicklistStatusHistories { get; set; }
    public ICollection<PicklistStatus> PicklistStatuses { get; set; }
    public ICollection<Product> ProductCreatedByUserLoginNavigations { get; set; }
    public ICollection<ProductFeaturePrice> ProductFeaturePriceCreatedByUserLoginNavigations { get; set; }
    public ICollection<ProductFeaturePrice> ProductFeaturePriceLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<Product> ProductLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<ProductPriceChange> ProductPriceChanges { get; set; }
    public ICollection<ProductPrice> ProductPriceCreatedByUserLoginNavigations { get; set; }
    public ICollection<ProductPrice> ProductPriceLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<ProductPromoCode> ProductPromoCodeCreatedByUserLoginNavigations { get; set; }
    public ICollection<ProductPromoCode> ProductPromoCodeLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<ProductPromo> ProductPromoCreatedByUserLoginNavigations { get; set; }
    public ICollection<ProductPromo> ProductPromoLastModifiedByUserLoginNavigations { get; set; }
    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
    public ICollection<RequirementStatus> RequirementStatuses { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
    public ICollection<ReturnStatus> ReturnStatuses { get; set; }
    public ICollection<SalesForecast> SalesForecastCreatedByUserLogins { get; set; }
    public ICollection<SalesForecastHistory> SalesForecastHistories { get; set; }
    public ICollection<SalesForecast> SalesForecastModifiedByUserLogins { get; set; }
    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
    public ICollection<SalesOpportunityHistory> SalesOpportunityHistories { get; set; }
    public ICollection<ShipmentReceipt> ShipmentReceipts { get; set; }
    public ICollection<ShipmentStatus> ShipmentStatuses { get; set; }
    public ICollection<TestingStatus> TestingStatuses { get; set; }
    public ICollection<Timesheet> Timesheets { get; set; }
    public ICollection<UserLoginHistory> UserLoginHistories { get; set; }
    public ICollection<UserLoginPasswordHistory> UserLoginPasswordHistories { get; set; }
    public ICollection<UserLoginSecurityGroup> UserLoginSecurityGroups { get; set; }
    public ICollection<WebUserPreference> WebUserPreferences { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignments { get; set; }
    public ICollection<WorkEffortReview> WorkEffortReviews { get; set; }
    public ICollection<WorkEffortStatus> WorkEffortStatuses { get; set; }
}