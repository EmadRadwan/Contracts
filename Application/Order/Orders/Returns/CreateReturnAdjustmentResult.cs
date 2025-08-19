public class CreateReturnAdjustmentResult
{
    // Renamed properties to avoid duplicate names
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public string ReturnAdjustmentId { get; private set; }

    // Factory method for success result
    public static CreateReturnAdjustmentResult Success(string returnAdjustmentId)
    {
        return new CreateReturnAdjustmentResult
        {
            IsSuccess = true, // Updated to use "IsSuccess"
            ReturnAdjustmentId = returnAdjustmentId
        };
    }

    // Factory method for error result
    public static CreateReturnAdjustmentResult Error(string message)
    {
        return new CreateReturnAdjustmentResult
        {
            IsSuccess = false, // Updated to use "IsSuccess"
            Message = message
        };
    }
}