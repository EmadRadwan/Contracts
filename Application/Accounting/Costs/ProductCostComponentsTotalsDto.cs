using Application.Manufacturing;

namespace Application.Accounting.Costs;

public class ProductCostComponentsTotalsDto
{
    public List<CostComponentDto> CostComponents { get; set; } = new();
    public int DirectLaborCount { get; set; }
    public int MaterialCostCount { get; set; }
    public int FohCostCount { get; set; }
}
