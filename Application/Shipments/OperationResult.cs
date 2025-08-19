namespace Application.Shipments;

public class OperationResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public object Data { get; private set; }

    // Private constructor to control instantiation through static methods
    private OperationResult(bool isSuccess, string message = null, object data = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }

    // Static method to create a successful result
    public static OperationResult Success(string message = null, object data = null)
    {
        return new OperationResult(true, message, data);
    }

    // Static method to create a failed result
    public static OperationResult Failure(string message, object data = null)
    {
        return new OperationResult(false, message, data);
    }

    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure: {Message}";
    }
}
