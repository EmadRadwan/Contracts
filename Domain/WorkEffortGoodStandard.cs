using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortGoodStandard
{
    public string WorkEffortId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string WorkEffortGoodStdTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public double? EstimatedQuantity { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product Product { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public WorkEffort WorkEffort { get; set; } = null!;
    public WorkEffortGoodStandardType WorkEffortGoodStdType { get; set; } = null!;
}