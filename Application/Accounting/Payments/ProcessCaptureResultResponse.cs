namespace Application.Accounting.Payments;


public class ProcessCaptureResultResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}