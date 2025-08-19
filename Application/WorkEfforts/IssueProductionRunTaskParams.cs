namespace Application.WorkEfforts;

public class IssueProductionRunTaskParams
{
    public string? WorkEffortId { get; set; }
    public string? ReserveOrderEnumId { get; set; }
    public string? FailIfItemsAreNotAvailable { get; set; }
    public string? FailIfItemsAreNotOnHand { get; set; }
}