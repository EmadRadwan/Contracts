namespace Application.Manufacturing;

public class ReservedComponentDto
{
    public string ProductId { get; set; } = null!;
    public decimal RequestedQty { get; set; }
    public decimal ReservedQty { get; set; }
}
