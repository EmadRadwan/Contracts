using System.ComponentModel.DataAnnotations;

namespace Application.Manufacturing;

public class CostComponentCalcDto
{
    public string CostComponentCalcId { get; set; } = null!;

    public string? Description { get; set; }
    public string? CostGlAccountTypeId { get; set; }
    public string? ProductId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? OffsettingGlAccountTypeId { get; set; }
    public decimal? FixedCost { get; set; }
    public decimal? VariableCost { get; set; }
    public decimal? PerMilliSecond { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? CostCustomMethodId { get; set; }
    public string? CostComponentTypeId { get; set; }
    public string? CostComponentTypeDescription { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}