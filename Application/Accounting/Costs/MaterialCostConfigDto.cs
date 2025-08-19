namespace Application.Shipments.Costs;

public class MaterialCostConfigDto
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public string UomId { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public string CostComponentTypeId { get; set; }
    public string CostComponentTypeDescription { get; set; }
    public string CostUomId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}