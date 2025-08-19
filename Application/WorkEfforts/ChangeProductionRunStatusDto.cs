namespace Application.WorkEfforts;

public class ChangeProductionRunStatusDto
{
    public string? ProductionRunId { get; set; }
    public string? StatusId { get; set; }
    public string? StartAllTasks { get; set; }
}