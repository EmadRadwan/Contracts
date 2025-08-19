using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class CostComponentType
{
    public CostComponentType()
    {
        CostComponentTypeAttrs = new HashSet<CostComponentTypeAttr>();
        CostComponents = new HashSet<CostComponent>();
        InverseParentType = new HashSet<CostComponentType>();
        ProductCostComponentCalcs = new HashSet<ProductCostComponentCalc>();
        WorkEffortCostCalcs = new HashSet<WorkEffortCostCalc>();
    }

    public string CostComponentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? SizeRelationClassification { get; set; } // e.g., Fixed, Variable, Semi-Variable
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CostComponentType? ParentType { get; set; }
    public ICollection<CostComponentTypeAttr> CostComponentTypeAttrs { get; set; }
    public ICollection<CostComponent> CostComponents { get; set; }
    public ICollection<CostComponentType> InverseParentType { get; set; }
    public ICollection<ProductCostComponentCalc> ProductCostComponentCalcs { get; set; }
    public ICollection<WorkEffortCostCalc> WorkEffortCostCalcs { get; set; }
}