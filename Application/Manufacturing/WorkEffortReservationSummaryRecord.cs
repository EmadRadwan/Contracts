using System.ComponentModel.DataAnnotations;
using Application.Catalog.Products;

namespace Application.Manufacturing;

public class WorkEffortReservationSummaryRecord
{
    [Key] public string ReservationWorkEffortId { get; set; } // First Task WorkEffortId
    [Key] public string ProductionRunWorkEffortId { get; set; }
    public string? ProductionRunName { get; set; }
    public string? FacilityName { get; set; }
    public decimal TotalReservedQuantity { get; set; } // Sum of reserved quantities
    public string? ProductName { get; set; }
    public string? CurrentStatusDescription { get; set; }
}