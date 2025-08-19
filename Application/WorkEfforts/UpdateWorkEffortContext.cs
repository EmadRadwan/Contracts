namespace Application.WorkEfforts;

public class UpdateWorkEffortContext
{
    public string WorkEffortId { get; set; }
    public double? ActualMilliSeconds { get; set; }
    public double? ActualSetupMillis { get; set; }
    public decimal QuantityProduced { get; set; }
    public decimal QuantityRejected { get; set; }
}
