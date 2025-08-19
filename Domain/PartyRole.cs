using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyRole
{
    public PartyRole()
    {
        AgreementPartyRoleNavigations = new HashSet<Agreement>();
        AgreementPartyRoles = new HashSet<Agreement>();
        AgreementRoles = new HashSet<AgreementRole>();
        BillingAccountRoles = new HashSet<BillingAccountRole>();
        BudgetRoles = new HashSet<BudgetRole>();
        CarrierShipmentMethods = new HashSet<CarrierShipmentMethod>();
        CommunicationEventRoles = new HashSet<CommunicationEventRole>();
        ContentRoles = new HashSet<ContentRole>();
        CustRequestParties = new HashSet<CustRequestParty>();
        DataResourceRoles = new HashSet<DataResourceRole>();
        EmploymentPartyRoleNavigations = new HashSet<Employment>();
        EmploymentPartyRoles = new HashSet<Employment>();
        FacilityGroupRoles = new HashSet<FacilityGroupRole>();
        FacilityParties = new HashSet<FacilityParty>();
        FinAccountRoles = new HashSet<FinAccountRole>();
        GlAccountRoles = new HashSet<GlAccountRole>();
        InvoiceRoles = new HashSet<InvoiceRole>();
        ItemIssuanceRoles = new HashSet<ItemIssuanceRole>();
        MarketingCampaignRoles = new HashSet<MarketingCampaignRole>();
        OrderItemRoles = new HashSet<OrderItemRole>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        OrderRoles = new HashSet<OrderRole>();
        PartyBenefitPartyRoleNavigations = new HashSet<PartyBenefit>();
        PartyBenefitPartyRoles = new HashSet<PartyBenefit>();
        PartyContactMeches = new HashSet<PartyContactMech>();
        PartyFixedAssetAssignments = new HashSet<PartyFixedAssetAssignment>();
        PartyGlAccounts = new HashSet<PartyGlAccount>();
        PartyRelationshipPartyRoleNavigations = new HashSet<PartyRelationship>();
        PartyRelationshipPartyRoles = new HashSet<PartyRelationship>();
        PayrollPreferences = new HashSet<PayrollPreference>();
        PerfReviewItems = new HashSet<PerfReviewItem>();
        PerfReviews = new HashSet<PerfReview>();
        PerformanceNotes = new HashSet<PerformanceNote>();
        PicklistRoles = new HashSet<PicklistRole>();
        ProdCatalogRoles = new HashSet<ProdCatalogRole>();
        ProductCategoryRoles = new HashSet<ProductCategoryRole>();
        ProductRoles = new HashSet<ProductRole>();
        ProductStoreGroupRoles = new HashSet<ProductStoreGroupRole>();
        ProductStoreRoles = new HashSet<ProductStoreRole>();
        QuoteRoles = new HashSet<QuoteRole>();
        RequirementRoles = new HashSet<RequirementRole>();
        SalesOpportunityRoles = new HashSet<SalesOpportunityRole>();
        SegmentGroupRoles = new HashSet<SegmentGroupRole>();
        ShipmentReceiptRoles = new HashSet<ShipmentReceiptRole>();
        TimesheetRoles = new HashSet<TimesheetRole>();
        WebSiteRoles = new HashSet<WebSiteRole>();
        WorkEffortPartyAssignments = new HashSet<WorkEffortPartyAssignment>();
    }

    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public RoleType RoleType { get; set; } = null!;
    public ICollection<Agreement> AgreementPartyRoleNavigations { get; set; }
    public ICollection<Agreement> AgreementPartyRoles { get; set; }
    public ICollection<AgreementRole> AgreementRoles { get; set; }
    public ICollection<BillingAccountRole> BillingAccountRoles { get; set; }
    public ICollection<BudgetRole> BudgetRoles { get; set; }
    public ICollection<CarrierShipmentMethod> CarrierShipmentMethods { get; set; }
    public ICollection<CommunicationEventRole> CommunicationEventRoles { get; set; }
    public ICollection<ContentRole> ContentRoles { get; set; }
    public ICollection<CustRequestParty> CustRequestParties { get; set; }
    public ICollection<DataResourceRole> DataResourceRoles { get; set; }
    public ICollection<Employment> EmploymentPartyRoleNavigations { get; set; }
    public ICollection<Employment> EmploymentPartyRoles { get; set; }
    public ICollection<FacilityGroupRole> FacilityGroupRoles { get; set; }
    public ICollection<FacilityParty> FacilityParties { get; set; }
    public ICollection<FinAccountRole> FinAccountRoles { get; set; }
    public ICollection<GlAccountRole> GlAccountRoles { get; set; }
    public ICollection<InvoiceRole> InvoiceRoles { get; set; }
    public ICollection<ItemIssuanceRole> ItemIssuanceRoles { get; set; }
    public ICollection<MarketingCampaignRole> MarketingCampaignRoles { get; set; }
    public ICollection<OrderItemRole> OrderItemRoles { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<OrderRole> OrderRoles { get; set; }
    public ICollection<PartyBenefit> PartyBenefitPartyRoleNavigations { get; set; }
    public ICollection<PartyBenefit> PartyBenefitPartyRoles { get; set; }
    public ICollection<PartyContactMech> PartyContactMeches { get; set; }
    public ICollection<PartyFixedAssetAssignment> PartyFixedAssetAssignments { get; set; }
    public ICollection<PartyGlAccount> PartyGlAccounts { get; set; }
    public ICollection<PartyRelationship> PartyRelationshipPartyRoleNavigations { get; set; }
    public ICollection<PartyRelationship> PartyRelationshipPartyRoles { get; set; }
    public ICollection<PayrollPreference> PayrollPreferences { get; set; }
    public ICollection<PerfReviewItem> PerfReviewItems { get; set; }
    public ICollection<PerfReview> PerfReviews { get; set; }
    public ICollection<PerformanceNote> PerformanceNotes { get; set; }
    public ICollection<PicklistRole> PicklistRoles { get; set; }
    public ICollection<ProdCatalogRole> ProdCatalogRoles { get; set; }
    public ICollection<ProductCategoryRole> ProductCategoryRoles { get; set; }
    public ICollection<ProductRole> ProductRoles { get; set; }
    public ICollection<ProductStoreGroupRole> ProductStoreGroupRoles { get; set; }
    public ICollection<ProductStoreRole> ProductStoreRoles { get; set; }
    public ICollection<QuoteRole> QuoteRoles { get; set; }
    public ICollection<RequirementRole> RequirementRoles { get; set; }
    public ICollection<SalesOpportunityRole> SalesOpportunityRoles { get; set; }
    public ICollection<SegmentGroupRole> SegmentGroupRoles { get; set; }
    public ICollection<ShipmentReceiptRole> ShipmentReceiptRoles { get; set; }
    public ICollection<TimesheetRole> TimesheetRoles { get; set; }
    public ICollection<WebSiteRole> WebSiteRoles { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignments { get; set; }
}