namespace Application.Catalog.Products;

public class CostComponentCalcDto
{
    public string WorkEffortId { get; set; }
    public string WorkEffortName { get; set; }
    public string CostComponentId { get; set; }
    public string CostComponentTypeId { get; set; }
    public string CostComponentTypeDescription { get; set; }
    public string ProductId { get; set; }
    public string ProductFeatureId { get; set; }
    public string PartyId { get; set; }
    public decimal Cost { get; set; }

    // Additional properties from CostComponentCalcs
    public string Description { get; set; }
    public decimal VariableCost { get; set; }
    public decimal PermilliSecond { get; set; }
    public string CurrencyUomId { get; set; }
}
