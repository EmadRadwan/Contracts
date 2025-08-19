using Domain;

namespace Application.Manufacturing;

public class GetWorkEffortCostsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public List<CostComponent> CostComponents { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? TotalCostNoMaterials { get; set; }
}