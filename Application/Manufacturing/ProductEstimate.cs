namespace Application.Manufacturing;

public class ProductEstimate
{
    public string ProductId { get; set; } = null!;
    public List<DateEstimate> DateEstimates { get; set; } = new List<DateEstimate>();
}