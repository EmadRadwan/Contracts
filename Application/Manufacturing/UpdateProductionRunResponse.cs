using Application.Catalog.Products;

namespace Application.Manufacturing;

public class UpdateProductionRunResponse
{
    public string ProductionRunId { get; set; }
    public string CurrentStatusDescription { get; set; }
    public ProductLovDto ProductId { get; set; }
    public string ProductName { get; set; }
    public string FacilityName { get; set; }
    public decimal Quantity { get; set; }
    public DateTime EstimatedCompletionDate { get; set; }
    public DateTime EstimatedStartDate { get; set; }
}