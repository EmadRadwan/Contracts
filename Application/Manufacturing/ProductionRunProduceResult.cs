namespace Application.Manufacturing;

public class ProductionRunProduceResult
{
    public List<string> InventoryItemIds { get; set; } = new List<string>();
    public List<string> Errors { get; set; } = new List<string>();
    public bool Success { get; set; } = false;
}