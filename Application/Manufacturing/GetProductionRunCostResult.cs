namespace Application.Manufacturing;

public class GetProductionRunCostResult
{
    public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public decimal? TotalCost { get; set; }
}