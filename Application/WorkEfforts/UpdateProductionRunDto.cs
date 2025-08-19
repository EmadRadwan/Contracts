namespace Application.WorkEfforts
{
    public class UpdateProductionRunDto
    {
        public string ProductionRunId { get; set; }          // Added to match the method parameter
        public decimal? Quantity { get; set; }               // Nullable to match the method parameter
        public DateTime? EstimatedStartDate { get; set; }    // Nullable to match the method parameter
        public string WorkEffortName { get; set; }           // Optional, so it's nullable in the method
        public string? Description { get; set; }              // Optional, so it's nullable in the method
        public string FacilityId { get; set; }               // Required, matching the method
    }
}
