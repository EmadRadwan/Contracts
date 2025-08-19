namespace Application.Order.Orders;

public class OperationResult
{
    // Indicates whether the operation was successful
    public bool IsSuccessful { get; private set; }

    // Message providing details about the operation's result
    public string Message { get; private set; }

    // Private constructor to control instantiation
    private OperationResult(bool isSuccessful, string message)
    {
        IsSuccessful = isSuccessful;
        Message = message;
    }

    // Factory method for creating a success result
    public static OperationResult CreateSuccess(string message = "Operation completed successfully.")
    {
        return new OperationResult(true, message);
    }

    // Factory method for creating a failure result
    public static OperationResult CreateFailure(string message)
    {
        return new OperationResult(false, message);
    }
}
