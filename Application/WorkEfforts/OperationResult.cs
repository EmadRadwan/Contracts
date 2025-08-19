namespace Application.WorkEfforts;

public class OperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }

    public OperationResult()
    {
        Success = false; // Default to failure
        ErrorMessage = string.Empty;
    }
}
