using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductCostComponentCalc
{
    public string ProductId { get; set; } = null!;
    public string CostComponentTypeId { get; set; } = null!;
    public string? CostComponentCalcId { get; set; }
    public DateTime FromDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CostComponentCalc? CostComponentCalc { get; set; }
    public CostComponentType CostComponentType { get; set; } = null!;
    public Product Product { get; set; } = null!;
}