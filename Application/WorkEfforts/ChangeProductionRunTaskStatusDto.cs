namespace Application.WorkEfforts;

public class ChangeProductionRunTaskStatusDto
{
    public string? ProductionRunId { get; set; }
    public string? TaskId { get; set; }
    public string? StatusId { get; set; }
    public bool? IssueAllComponents { get; set; }
    public string? StartAllTasks { get; set; }
}