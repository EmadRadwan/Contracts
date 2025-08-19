using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffort
{
    public WorkEffort()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AgreementWorkEffortApplics = new HashSet<AgreementWorkEffortApplic>();
        CommunicationEventWorkEffs = new HashSet<CommunicationEventWorkEff>();
        CostComponents = new HashSet<CostComponent>();
        CustRequestItemWorkEfforts = new HashSet<CustRequestItemWorkEffort>();
        CustRequestWorkEfforts = new HashSet<CustRequestWorkEffort>();
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        InventoryItemDetails = new HashSet<InventoryItemDetail>();
        InverseWorkEffortParent = new HashSet<WorkEffort>();
        OrderHeaderWorkEfforts = new HashSet<OrderHeaderWorkEffort>();
        PersonTrainings = new HashSet<PersonTraining>();
        ProductAssocs = new HashSet<ProductAssoc>();
        ProductMaints = new HashSet<ProductMaint>();
        QuoteItems = new HashSet<QuoteItem>();
        QuoteWorkEfforts = new HashSet<QuoteWorkEffort>();
        RateAmounts = new HashSet<RateAmount>();
        SalesOpportunityWorkEfforts = new HashSet<SalesOpportunityWorkEffort>();
        ShipmentEstimatedArrivalWorkEffs = new HashSet<Shipment>();
        ShipmentEstimatedShipWorkEffs = new HashSet<Shipment>();
        ShoppingListWorkEfforts = new HashSet<ShoppingListWorkEffort>();
        TimeEntries = new HashSet<TimeEntry>();
        WorkEffortAssocWorkEffortIdFromNavigations = new HashSet<WorkEffortAssoc>();
        WorkEffortAssocWorkEffortIdToNavigations = new HashSet<WorkEffortAssoc>();
        WorkEffortAttributes = new HashSet<WorkEffortAttribute>();
        WorkEffortBillings = new HashSet<WorkEffortBilling>();
        WorkEffortContactMechNews = new HashSet<WorkEffortContactMechNew>();
        WorkEffortContents = new HashSet<WorkEffortContent>();
        WorkEffortCostCalcs = new HashSet<WorkEffortCostCalc>();
        WorkEffortDeliverableProds = new HashSet<WorkEffortDeliverableProd>();
        WorkEffortEventReminders = new HashSet<WorkEffortEventReminder>();
        WorkEffortFixedAssetAssigns = new HashSet<WorkEffortFixedAssetAssign>();
        WorkEffortFixedAssetStds = new HashSet<WorkEffortFixedAssetStd>();
        WorkEffortGoodStandards = new HashSet<WorkEffortGoodStandard>();
        WorkEffortInventoryAssigns = new HashSet<WorkEffortInventoryAssign>();
        WorkEffortInventoryProduceds = new HashSet<WorkEffortInventoryProduced>();
        WorkEffortKeywords = new HashSet<WorkEffortKeyword>();
        WorkEffortNotes = new HashSet<WorkEffortNote>();
        WorkEffortPartyAssignments = new HashSet<WorkEffortPartyAssignment>();
        WorkEffortReviews = new HashSet<WorkEffortReview>();
        WorkEffortSkillStandards = new HashSet<WorkEffortSkillStandard>();
        WorkEffortStatuses = new HashSet<WorkEffortStatus>();
        WorkEffortSurveyAppls = new HashSet<WorkEffortSurveyAppl>();
        WorkEffortTransBoxes = new HashSet<WorkEffortTransBox>();
        WorkOrderItemFulfillments = new HashSet<WorkOrderItemFulfillment>();
        WorkRequirementFulfillments = new HashSet<WorkRequirementFulfillment>();
        WorkEffortInventoryRes = new HashSet<WorkEffortInventoryRes>();
    }

    public string WorkEffortId { get; set; } = null!;
    public string? WorkEffortTypeId { get; set; }
    public string? CurrentStatusId { get; set; }
    public DateTime? LastStatusUpdate { get; set; }
    public string? WorkEffortPurposeTypeId { get; set; }
    public string? WorkEffortParentId { get; set; }
    public string? ScopeEnumId { get; set; }
    public int? Priority { get; set; }
    public int? PercentComplete { get; set; }
    public string? WorkEffortName { get; set; }
    public string? ShowAsEnumId { get; set; }
    public string? SendNotificationEmail { get; set; }
    public string? Description { get; set; }
    public string? LocationDesc { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public double? EstimatedMilliSeconds { get; set; }
    public double? EstimatedSetupMillis { get; set; }
    public string? EstimateCalcMethod { get; set; }
    public double? ActualMilliSeconds { get; set; }
    public double? ActualSetupMillis { get; set; }
    public double? TotalMilliSecondsAllowed { get; set; }
    public decimal? TotalMoneyAllowed { get; set; }
    public string? MoneyUomId { get; set; }
    public string? SpecialTerms { get; set; }
    public int? TimeTransparency { get; set; }
    public string? UniversalId { get; set; }
    public string? SourceReferenceId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? FacilityId { get; set; }
    public string? InfoUrl { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public string? TempExprId { get; set; }
    public string? RuntimeDataId { get; set; }
    public string? NoteId { get; set; }
    public string? ServiceLoaderName { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public decimal? QuantityProduced { get; set; }
    public decimal? QuantityRejected { get; set; }
    public decimal? ReservPersons { get; set; }
    public decimal? Reserv2ndPPPerc { get; set; }
    public decimal? ReservNthPPPerc { get; set; }
    public string? AccommodationMapId { get; set; }
    public string? AccommodationSpotId { get; set; }
    public int? RevisionNumber { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AccommodationMap? AccommodationMap { get; set; }
    public AccommodationSpot? AccommodationSpot { get; set; }
    public StatusItem? CurrentStatus { get; set; }
    public CustomMethod? EstimateCalcMethodNavigation { get; set; }
    public Facility? Facility { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public Uom? MoneyUom { get; set; }
    public NoteDatum? Note { get; set; }
    public RecurrenceInfo? RecurrenceInfo { get; set; }
    public RuntimeDatum? RuntimeData { get; set; }
    public Enumeration? ScopeEnum { get; set; }
    public TemporalExpression? TempExpr { get; set; }
    public WorkEffort? WorkEffortParent { get; set; }
    public WorkEffortPurposeType? WorkEffortPurposeType { get; set; }
    public WorkEffortType? WorkEffortType { get; set; }
    public WorkEffortIcalDatum WorkEffortIcalDatum { get; set; } = null!;
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AgreementWorkEffortApplic> AgreementWorkEffortApplics { get; set; }
    public ICollection<CommunicationEventWorkEff> CommunicationEventWorkEffs { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<CustRequestItemWorkEffort> CustRequestItemWorkEfforts { get; set; }
    public ICollection<CustRequestWorkEffort> CustRequestWorkEfforts { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<InventoryItemDetail> InventoryItemDetails { get; set; }
    public ICollection<WorkEffort> InverseWorkEffortParent { get; set; }
    public ICollection<OrderHeaderWorkEffort> OrderHeaderWorkEfforts { get; set; }
    public ICollection<PersonTraining> PersonTrainings { get; set; }
    public ICollection<ProductAssoc> ProductAssocs { get; set; }
    public ICollection<ProductMaint> ProductMaints { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<QuoteWorkEffort> QuoteWorkEfforts { get; set; }
    public ICollection<RateAmount> RateAmounts { get; set; }
    public ICollection<SalesOpportunityWorkEffort> SalesOpportunityWorkEfforts { get; set; }
    public ICollection<Shipment> ShipmentEstimatedArrivalWorkEffs { get; set; }
    public ICollection<Shipment> ShipmentEstimatedShipWorkEffs { get; set; }
    public ICollection<ShoppingListWorkEffort> ShoppingListWorkEfforts { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; }
    public ICollection<WorkEffortAssoc> WorkEffortAssocWorkEffortIdFromNavigations { get; set; }
    public ICollection<WorkEffortAssoc> WorkEffortAssocWorkEffortIdToNavigations { get; set; }
    public ICollection<WorkEffortAttribute> WorkEffortAttributes { get; set; }
    public ICollection<WorkEffortBilling> WorkEffortBillings { get; set; }
    public ICollection<WorkEffortContactMechNew> WorkEffortContactMechNews { get; set; }
    public ICollection<WorkEffortContent> WorkEffortContents { get; set; }
    public ICollection<WorkEffortCostCalc> WorkEffortCostCalcs { get; set; }
    public ICollection<WorkEffortDeliverableProd> WorkEffortDeliverableProds { get; set; }
    public ICollection<WorkEffortEventReminder> WorkEffortEventReminders { get; set; }
    public ICollection<WorkEffortFixedAssetAssign> WorkEffortFixedAssetAssigns { get; set; }
    public ICollection<WorkEffortFixedAssetStd> WorkEffortFixedAssetStds { get; set; }
    public ICollection<WorkEffortGoodStandard> WorkEffortGoodStandards { get; set; }
    public ICollection<WorkEffortInventoryAssign> WorkEffortInventoryAssigns { get; set; }
    public ICollection<WorkEffortInventoryProduced> WorkEffortInventoryProduceds { get; set; }
    public ICollection<WorkEffortKeyword> WorkEffortKeywords { get; set; }
    public ICollection<WorkEffortNote> WorkEffortNotes { get; set; }
    public ICollection<WorkEffortPartyAssignment> WorkEffortPartyAssignments { get; set; }
    public ICollection<WorkEffortReview> WorkEffortReviews { get; set; }
    public ICollection<WorkEffortSkillStandard> WorkEffortSkillStandards { get; set; }
    public ICollection<WorkEffortStatus> WorkEffortStatuses { get; set; }
    public ICollection<WorkEffortSurveyAppl> WorkEffortSurveyAppls { get; set; }
    public ICollection<WorkEffortTransBox> WorkEffortTransBoxes { get; set; }
    public ICollection<WorkOrderItemFulfillment> WorkOrderItemFulfillments { get; set; }
    public ICollection<WorkRequirementFulfillment> WorkRequirementFulfillments { get; set; }
    public ICollection<WorkEffortInventoryRes> WorkEffortInventoryRes { get; set; }
}