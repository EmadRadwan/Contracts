namespace Application.Manufacturing;

public class ProductionRunTaskProduceResult
{
    public List<string> InventoryItemIds { get; set; }
    public string ErrorMessage { get; set; }
    public bool HasError { get; set; }

    public ProductionRunTaskProduceResult()
    {
        InventoryItemIds = new List<string>();
        HasError = false;
        ErrorMessage = string.Empty;
    }
}