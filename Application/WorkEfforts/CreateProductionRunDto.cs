namespace Application.WorkEfforts;

public class CreateProductionRunDto
{
    public string ProductId { get; set; }
    public DateTime EstimatedStartDate { get; set; }
    public decimal QuantityToProduce { get; set; }
    public string FacilityId { get; set; }
    public string? RoutingId { get; set; }
    public string? WorkEffortName { get; set; }
    public string? Description { get; set; }
}