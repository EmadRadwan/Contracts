using System.ComponentModel.DataAnnotations;
using Application.Catalog.Products;

namespace Application.Manufacturing;

public class WorkEffortReservationRecord
{
    [Key] public string ReservationWorkEffortId { get; set; } // First Task WorkEffortId
    [Key] public string ProductionRunWorkEffortId { get; set; } // Main ProductionRun WorkEffortId
    public string? ProductionRunName { get; set; }
    public string? Description { get; set; }
    public string? FacilityId { get; set; }
    public string? FacilityName { get; set; }
    public decimal? QuantityToProduce { get; set; }
    public decimal? QuantityProduced { get; set; }
    public decimal? QuantityRejected { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    public string? CurrentStatusId { get; set; }
    public string? CurrentStatusDescription { get; set; }
    public string? UomAndQuantity { get; set; }
    public decimal TotalReservedQuantity { get; set; } // From WorkEffortInventoryRes
    public ProductLovDto? Product { get; set; }
    public string? ProductName { get; set; }
}