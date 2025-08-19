using Domain;

namespace Application.Manufacturing;

public class CustomMethodParameters
{
    public ProductCostComponentCalc ProductCostComponentCalc { get; set; }
    public CostComponentCalc CostComponentCalc { get; set; }
    public string CurrencyUomId { get; set; }
    public string CostComponentTypePrefix { get; set; }
    public decimal? BaseCost { get; set; } // Base cost passed to the custom method
}