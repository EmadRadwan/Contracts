public class CreateReturnItemResult
{
    public bool IsSuccess { get; private set; } // Renamed to avoid conflict with "Success"
    public string Message { get; private set; }
    public string ReturnItemSeqId { get; private set; }

    public static CreateReturnItemResult Success(string returnItemSeqId)
    {
        return new CreateReturnItemResult
        {
            IsSuccess = true, // Updated property name
            ReturnItemSeqId = returnItemSeqId
        };
    }

    public static CreateReturnItemResult Error(string message)
    {
        return new CreateReturnItemResult
        {
            IsSuccess = false, // Updated property name
            Message = message
        };
    }
}