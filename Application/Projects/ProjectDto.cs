namespace Application.Projects;

public class ProjectDto
{
    public string? WorkEffortId { get; set; }
    public string? ProjectNum { get; set; }
    public string? ProjectName { get; set; }
    public string? PartyId { get; set; }
    public string? WorkEffortTypeId { get; set; }
    public string? CurrentStatusId { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
}