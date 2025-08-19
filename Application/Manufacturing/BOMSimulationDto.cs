namespace Application.Manufacturing;

public class BOMSimulationDto
{
    public int ProductLevel { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? QOH { get; set; } 
    public decimal? Cost { get; set; }
    public decimal? TotalCost => ProductLevel == 0 ? Cost : Cost * Quantity;
    public string? UomId { get; set; }
    public string? UomDescription { get; set; }
    public bool IsTemplateLink { get; set; } // New: Flags template link
    public string Instruction { get; set; } 
}