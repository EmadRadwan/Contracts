namespace Application.WorkEfforts;

public class ProductionRunTaskReturnMaterialResult
{
    // A list of inventory item IDs that were returned
    public List<string> InventoryItemIds { get; set; }

    // A flag indicating whether an error occurred
    public bool HasError { get; set; }

    // A message that provides error details (if any)
    public string ErrorMessage { get; set; }

    public ProductionRunTaskReturnMaterialResult()
    {
        InventoryItemIds = new List<string>();
        HasError = false;
        ErrorMessage = string.Empty;
    }
}
