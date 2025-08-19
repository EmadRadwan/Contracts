namespace Domain;

public class Requirement
{
    public Requirement()
    {
        DesiredFeatures = new HashSet<DesiredFeature>();
        OrderRequirementCommitments = new HashSet<OrderRequirementCommitment>();
        RequirementAttributes = new HashSet<RequirementAttribute>();
        RequirementBudgetAllocations = new HashSet<RequirementBudgetAllocation>();
        RequirementCustRequests = new HashSet<RequirementCustRequest>();
        RequirementRoles = new HashSet<RequirementRole>();
        RequirementStatuses = new HashSet<RequirementStatus>();
        WorkRequirementFulfillments = new HashSet<WorkRequirementFulfillment>();
    }

    public string RequirementId { get; set; } = null!;
    public string? RequirementTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? DeliverableId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? ProductId { get; set; }
    public string? StatusId { get; set; }
    public string? Description { get; set; }
    public DateTime? RequirementStartDate { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public decimal? Quantity { get; set; }
    public string? UseCase { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? FacilityIdTo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Deliverable? Deliverable { get; set; }
    public Facility? Facility { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public Product? Product { get; set; }
    public RequirementType? RequirementType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<DesiredFeature> DesiredFeatures { get; set; }
    public ICollection<OrderRequirementCommitment> OrderRequirementCommitments { get; set; }
    public ICollection<RequirementAttribute> RequirementAttributes { get; set; }
    public ICollection<RequirementBudgetAllocation> RequirementBudgetAllocations { get; set; }
    public ICollection<RequirementCustRequest> RequirementCustRequests { get; set; }
    public ICollection<RequirementRole> RequirementRoles { get; set; }
    public ICollection<RequirementStatus> RequirementStatuses { get; set; }
    public ICollection<WorkRequirementFulfillment> WorkRequirementFulfillments { get; set; }
}