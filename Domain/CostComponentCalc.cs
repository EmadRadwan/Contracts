using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class CostComponentCalc
{
    public CostComponentCalc()
    {
        CostComponents = new HashSet<CostComponent>();
        ProductCostComponentCalcs = new HashSet<ProductCostComponentCalc>();
        WorkEffortCostCalcs = new HashSet<WorkEffortCostCalc>();
    }

    public string CostComponentCalcId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public string? CostGlAccountTypeId { get; set; }
    public string? OffsettingGlAccountTypeId { get; set; }
    public decimal? FixedCost { get; set; }
    public decimal? VariableCost { get; set; }
    public int? PerMilliSecond { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CostCustomMethodId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? CostCustomMethod { get; set; }
    public GlAccountType? CostGlAccountType { get; set; }
    public Uom? CurrencyUom { get; set; }
    public GlAccountType? OffsettingGlAccountType { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<ProductCostComponentCalc> ProductCostComponentCalcs { get; set; }
    public ICollection<WorkEffortCostCalc> WorkEffortCostCalcs { get; set; }
}