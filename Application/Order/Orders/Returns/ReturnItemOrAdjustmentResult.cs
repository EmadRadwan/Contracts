namespace Application.Order.Orders.Returns;

public class ReturnItemOrAdjustmentResult
{
    // Indicates whether the operation was successful
    public bool Success { get; private set; }

    // Message describing the result or error
    public string Message { get; private set; }

    // Optional data returned from the operation
    public object Data { get; private set; }

    // Factory method to create a successful result
    public static ReturnItemOrAdjustmentResult SuccessResult(object data = null)
    {
        return new ReturnItemOrAdjustmentResult
        {
            Success = true,
            Message = "Operation completed successfully.",
            Data = data
        };
    }

    // Factory method to create an error result
    public static ReturnItemOrAdjustmentResult Error(string message)
    {
        return new ReturnItemOrAdjustmentResult
        {
            Success = false,
            Message = message
        };
    }
}
