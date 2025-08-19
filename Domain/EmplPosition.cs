namespace Domain;

public class EmplPosition
{
    public EmplPosition()
    {
        EmplPositionFulfillments = new HashSet<EmplPositionFulfillment>();
        EmplPositionReportingStructEmplPositionIdManagedByNavigations = new HashSet<EmplPositionReportingStruct>();
        EmplPositionReportingStructEmplPositionIdReportingToNavigations = new HashSet<EmplPositionReportingStruct>();
        EmplPositionResponsibilities = new HashSet<EmplPositionResponsibility>();
    }

    public string EmplPositionId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? PartyId { get; set; }
    public string? BudgetId { get; set; }
    public string? BudgetItemSeqId { get; set; }
    public string? EmplPositionTypeId { get; set; }
    public DateTime? EstimatedFromDate { get; set; }
    public DateTime? EstimatedThruDate { get; set; }
    public string? SalaryFlag { get; set; }
    public string? ExemptFlag { get; set; }
    public string? FulltimeFlag { get; set; }
    public string? TemporaryFlag { get; set; }
    public DateTime? ActualFromDate { get; set; }
    public DateTime? ActualThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? Party { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<EmplPositionFulfillment> EmplPositionFulfillments { get; set; }

    public ICollection<EmplPositionReportingStruct> EmplPositionReportingStructEmplPositionIdManagedByNavigations
    {
        get;
        set;
    }

    public ICollection<EmplPositionReportingStruct> EmplPositionReportingStructEmplPositionIdReportingToNavigations
    {
        get;
        set;
    }

    public ICollection<EmplPositionResponsibility> EmplPositionResponsibilities { get; set; }
}