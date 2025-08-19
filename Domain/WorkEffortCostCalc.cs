using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortCostCalc
{
    public string WorkEffortId { get; set; } = null!;
    public string CostComponentTypeId { get; set; } = null!;
    public string? CostComponentCalcId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CostComponentCalc? CostComponentCalc { get; set; }
    public CostComponentType CostComponentType { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}