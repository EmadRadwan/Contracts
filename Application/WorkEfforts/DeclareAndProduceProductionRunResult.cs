public class DeclareAndProduceProductionRunResult
{
    public bool IsSuccessful { get; private set; }
    public string ErrorMessage { get; private set; }
    public List<string> InventoryItemIds { get; private set; }
    public decimal Quantity { get; private set; }

    // Private constructor to prevent direct instantiation
    private DeclareAndProduceProductionRunResult() { }

    // Static method for success result
    public static DeclareAndProduceProductionRunResult CreateSuccessResult(List<string> inventoryItemIds, decimal quantity)
    {
        return new DeclareAndProduceProductionRunResult
        {
            IsSuccessful = true,
            InventoryItemIds = inventoryItemIds,
            Quantity = quantity
        };
    }

    // Static method for failure result
    public static DeclareAndProduceProductionRunResult CreateFailureResult(string errorMessage)
    {
        return new DeclareAndProduceProductionRunResult
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}