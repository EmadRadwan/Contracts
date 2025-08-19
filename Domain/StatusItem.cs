using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class StatusItem
{
    public StatusItem()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        AgreementStatuses = new HashSet<AgreementStatus>();
        AllocationPlanHeaders = new HashSet<AllocationPlanHeader>();
        AllocationPlanItems = new HashSet<AllocationPlanItem>();
        BudgetStatuses = new HashSet<BudgetStatus>();
        CommunicationEventRoles = new HashSet<CommunicationEventRole>();
        CommunicationEvents = new HashSet<CommunicationEvent>();
        ContactListCommStatuses = new HashSet<ContactListCommStatus>();
        ContactListParties = new HashSet<ContactListParty>();
        ContentApprovals = new HashSet<ContentApproval>();
        ContentPurposeOperations = new HashSet<ContentPurposeOperation>();
        Contents = new HashSet<Content>();
        CustRequestItems = new HashSet<CustRequestItem>();
        CustRequestStatuses = new HashSet<CustRequestStatus>();
        CustRequests = new HashSet<CustRequest>();
        DataResources = new HashSet<DataResource>();
        EmplLeaves = new HashSet<EmplLeave>();
        EmplPositions = new HashSet<EmplPosition>();
        FinAccountStatuses = new HashSet<FinAccountStatus>();
        FinAccountTrans = new HashSet<FinAccountTran>();
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        GlReconciliations = new HashSet<GlReconciliation>();
        InventoryItemStatuses = new HashSet<InventoryItemStatus>();
        InventoryItems = new HashSet<InventoryItem>();
        InventoryTransfers = new HashSet<InventoryTransfer>();
        InvoiceStatuses = new HashSet<InvoiceStatus>();
        Invoices = new HashSet<Invoice>();
        JobSandboxes = new HashSet<JobSandbox>();
        MarketingCampaigns = new HashSet<MarketingCampaign>();
        OrderDeliverySchedules = new HashSet<OrderDeliverySchedule>();
        OrderHeaderStatuses = new HashSet<OrderHeader>();
        OrderHeaderSyncStatuses = new HashSet<OrderHeader>();
        OrderItemStatuses = new HashSet<OrderItem>();
        OrderItemSyncStatuses = new HashSet<OrderItem>();
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        OrderStatuses = new HashSet<OrderStatus>();
        Parties = new HashSet<Party>();
        PartyFixedAssetAssignments = new HashSet<PartyFixedAssetAssignment>();
        PartyInvitations = new HashSet<PartyInvitation>();
        PartyQualStatuses = new HashSet<PartyQual>();
        PartyQualVerifStatuses = new HashSet<PartyQual>();
        PartyRelationships = new HashSet<PartyRelationship>();
        PartyStatuses = new HashSet<PartyStatus>();
        Payments = new HashSet<Payment>();
        PicklistItems = new HashSet<PicklistItem>();
        PicklistStatusHistoryStatusIdToNavigations = new HashSet<PicklistStatusHistory>();
        PicklistStatusHistoryStatuses = new HashSet<PicklistStatusHistory>();
        PicklistStatusStatusIdToNavigations = new HashSet<PicklistStatus>();
        PicklistStatusStatuses = new HashSet<PicklistStatus>();
        Picklists = new HashSet<Picklist>();
        ProductGroupOrders = new HashSet<ProductGroupOrder>();
        ProductKeywordNews = new HashSet<ProductKeywordNew>();
        ProductReviews = new HashSet<ProductReview>();
        ProductStoreDigitalItemApprovedStatusNavigations = new HashSet<ProductStore>();
        ProductStoreHeaderApprovedStatusNavigations = new HashSet<ProductStore>();
        ProductStoreHeaderCancelStatusNavigations = new HashSet<ProductStore>();
        ProductStoreHeaderDeclinedStatusNavigations = new HashSet<ProductStore>();
        ProductStoreItemApprovedStatusNavigations = new HashSet<ProductStore>();
        ProductStoreItemCancelStatusNavigations = new HashSet<ProductStore>();
        ProductStoreItemDeclinedStatusNavigations = new HashSet<ProductStore>();
        Quotes = new HashSet<Quote>();
        RequirementStatuses = new HashSet<RequirementStatus>();
        Requirements = new HashSet<Requirement>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ReturnItemExpectedItemStatusNavigations = new HashSet<ReturnItem>();
        ReturnItemStatuses = new HashSet<ReturnItem>();
        ReturnStatuses = new HashSet<ReturnStatus>();
        ShipmentRouteSegments = new HashSet<ShipmentRouteSegment>();
        ShipmentStatuses = new HashSet<ShipmentStatus>();
        Shipments = new HashSet<Shipment>();
        StatusValidChangeStatusIdToNavigations = new HashSet<StatusValidChange>();
        StatusValidChangeStatuses = new HashSet<StatusValidChange>();
        SurveyResponses = new HashSet<SurveyResponse>();
        TestingStatuses = new HashSet<TestingStatus>();
        Timesheets = new HashSet<Timesheet>();
        WorkEffortFixedAssetAssignAvailabilityStatuses = new HashSet<WorkEffortFixedAssetAssign>();
        WorkEffortFixedAssetAssignStatuses = new HashSet<WorkEffortFixedAssetAssign>();
        WorkEffortGoodStandards = new HashSet<WorkEffortGoodStandard>();
        WorkEffortInventoryAssigns = new HashSet<WorkEffortInventoryAssign>();
        WorkEffortPartyAssignmentAvailabilityStatuses = new HashSet<WorkEffortPartyAssignment>();
        WorkEffortPartyAssignmentStatuses = new HashSet<WorkEffortPartyAssignment>();
        WorkEffortReviews = new HashSet<WorkEffortReview>();
        WorkEffortStatuses = new HashSet<WorkEffortStatus>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string StatusId { get; set; } = null!;
    public string? StatusTypeId { get; set; }
    public string? StatusCode { get; set; }
    public string? SequenceId { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusType? StatusType { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<AgreementStatus> AgreementStatuses { get; set; }
    public ICollection<AllocationPlanHeader> AllocationPlanHeaders { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItems { get; set; }
    public ICollection<BudgetStatus> BudgetStatuses { get; set; }
    public ICollection<CommunicationEventRole> CommunicationEventRoles { get; set; }
    public ICollection<CommunicationEvent> CommunicationEvents { get; set; }
    public ICollection<ContactListCommStatus> ContactListCommStatuses { get; set; }
    public ICollection<ContactListParty> ContactListParties { get; set; }
    public ICollection<ContentApproval> ContentApprovals { get; set; }
    public ICollection<ContentPurposeOperation> ContentPurposeOperations { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<CustRequestItem> CustRequestItems { get; set; }
    public ICollection<CustRequestStatus> CustRequestStatuses { get; set; }
    public ICollection<CustRequest> CustRequests { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<EmplLeave> EmplLeaves { get; set; }
    public ICollection<EmplPosition> EmplPositions { get; set; }
    public ICollection<FinAccountStatus> FinAccountStatuses { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<GlReconciliation> GlReconciliations { get; set; }
    public ICollection<InventoryItemStatus> InventoryItemStatuses { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<InventoryTransfer> InventoryTransfers { get; set; }
    public ICollection<InvoiceStatus> InvoiceStatuses { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<JobSandbox> JobSandboxes { get; set; }
    public ICollection<MarketingCampaign> MarketingCampaigns { get; set; }
    public ICollection<OrderDeliverySchedule> OrderDeliverySchedules { get; set; }
    public ICollection<OrderHeader> OrderHeaderStatuses { get; set; }
    public ICollection<OrderHeader> OrderHeaderSyncStatuses { get; set; }
    public ICollection<OrderItem> OrderItemStatuses { get; set; }
    public ICollection<OrderItem> OrderItemSyncStatuses { get; set; }
    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<OrderStatus> OrderStatuses { get; set; }
    public ICollection<Party> Parties { get; set; }
    public ICollection<PartyFixedAssetAssignment> PartyFixedAssetAssignments { get; set; }
    public ICollection<PartyInvitation> PartyInvitations { get; set; }
    public ICollection<PartyQual> PartyQualStatuses { get; set; }
    public ICollection<PartyQual> PartyQualVerifStatuses { get; set; }
    public ICollection<PartyRelationship> PartyRelationships { get; set; }
    public ICollection<PartyStatus> PartyStatuses { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<PicklistItem> PicklistItems { get; set; }
    public ICollection<PicklistStatusHistory> PicklistStatusHistoryStatusIdToNavigations { get; set; }
    public ICollection<PicklistStatusHistory> PicklistStatusHistoryStatuses { get; set; }
    public ICollection<PicklistStatus> PicklistStatusStatusIdToNavigations { get; set; }
    public ICollection<PicklistStatus> PicklistStatusStatuses { get; set; }
    public ICollection<Picklist> Picklists { get; set; }
    public ICollection<ProductGroupOrder> ProductGroupOrders { get; set; }
    public ICollection<ProductKeywordNew> ProductKeywordNews { get; set; }
    public ICollection<ProductReview> ProductReviews { get; set; }
    public ICollection<ProductStore> ProductStoreDigitalItemApprovedStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreHeaderApprovedStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreHeaderCancelStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreHeaderDeclinedStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreItemApprovedStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreItemCancelStatusNavigations { get; set; }
    public ICollection<ProductStore> ProductStoreItemDeclinedStatusNavigations { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<RequirementStatus> RequirementStatuses { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<ReturnItem> ReturnItemExpectedItemStatusNavigations { get; set; }
    public ICollection<ReturnItem> ReturnItemStatuses { get; set; }
    public ICollection<ReturnStatus> ReturnStatuses { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegments { get; set; }
    public ICollection<ShipmentStatus> ShipmentStatuses { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
    public ICollection<StatusValidChange> StatusValidChangeStatusIdToNavigations { get; set; }
    public ICollection<StatusValidChange> StatusValidChangeStatuses { get; set; }
    public ICollection<SurveyResponse> SurveyResponses { get; set; }
    public ICollection<TestingStatus> TestingStatuses { get; set; }
    public ICollection<Timesheet> Timesheets { get; set; }
    public ICollection<WorkEffortFixedAssetAssign> WorkEffortFixedAssetAssignAvailabilityStatuses { get; set; }
    public ICollection<WorkEffortFixedAssetAssign> WorkEffortFixedAssetAssignStatuses { get; set; }
    public ICollection<WorkEffortGoodStandard> WorkEffortGoodStandards { get; set; }
    public ICollection<WorkEffortInventoryAssign> WorkEffortInventoryAssigns { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignmentAvailabilityStatuses { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignmentStatuses { get; set; }
    public ICollection<WorkEffortReview> WorkEffortReviews { get; set; }
    public ICollection<WorkEffortStatus> WorkEffortStatuses { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}