namespace Application.WorkEfforts
{
    public class WorkEffortReservationDto
    {
        public string WorkEffortId { get; set; } = null!;
        public string? WorkEffortName { get; set; }
        public string? ProductionRunId { get; set; }
        public string? Description { get; set; }
        public string? FacilityId { get; set; }
        public DateTime? EstimatedStartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public decimal TotalReservedQuantity { get; set; }
        public string CurrentStatusId { get; set; } = null!;
    }
}