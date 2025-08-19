namespace Application.Manufacturing
{
    public class ReserveProductionRunTaskParams
    {
        public string WorkEffortId { get; set; } = null!;
        public string? RequireInventory { get; set; } = "Y";
    }
}
