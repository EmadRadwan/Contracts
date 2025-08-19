namespace Application.WorkEfforts;

public class ReturnProductionRunTaskMaterialDto
{
    public string WorkEffortId { get; set; }
    public string ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string LotId { get; set; }
}
