namespace Application.WorkEfforts;

public class UpdateProductionRunTaskContext
{
    public string ProductionRunId { get; set; }
    public string ProductionRunTaskId { get; set; }
    public string? PartyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? AddQuantityProduced { get; set; }
    public decimal? AddQuantityRejected { get; set; }
    public decimal? AddSetupTime { get; set; }
    public decimal? AddTaskTime { get; set; }
    public string? Comments { get; set; }
    public bool? IssueRequiredComponents { get; set; }
    public List<ComponentLocation>? ComponentLocations { get; set; }
   
}