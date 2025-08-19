using Application.Manufacturing;
using Domain;

namespace Application.WorkEfforts;

public class ProductionRunDetailsDto
{
    public string ProductionRunId { get; set; }
    public string WorkEffortId { get; set; }
    public double QuantityToProduce { get; set; }
    public double QuantityProduced { get; set; }
    public double QuantityRejected { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string ProductionRunName { get; set; }
    public string Description { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public string CurrentStatusId { get; set; }
    public string FacilityId { get; set; }
    public string? ManufacturerId { get; set; }
    public double? Quantity { get; set; }
    public int? Priority { get; set; }

    public string? CanProduce { get; set; }
    public string CanDeclareAndProduce { get; set; } = "N";
    public string? LastLotId { get; set; }
    public List<WorkOrderItemFulfillment> OrderItems { get; set; }
    public List<WorkEffortInventoryProduced> InventoryItems { get; set; }
    public List<ProductionRunRoutingTaskDto> ProductionRunRoutingTasks { get; set; }
    public List<ProductionRunComponentDto> ProductionRunComponents { get; set; }
    public string StartTaskId { get; set; }
    public string IssueTaskId { get; set; }
    public string CompleteTaskId { get; set; }
    public string ProductId { get; set; }  // Added ProductId property
    public string ProductName { get; set; }  // Added ProductName property
}
