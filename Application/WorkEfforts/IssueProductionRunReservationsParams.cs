namespace Application.WorkEfforts;

public class IssueProductionRunReservationsParams
{
    public string WorkEffortId { get; set; } = null!;
    public bool FailIfNotEnoughQoh { get; set; } = true;
    public string? ReasonEnumId { get; set; }
    public string? Description { get; set; }
}