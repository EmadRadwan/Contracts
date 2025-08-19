namespace Application.Accounting.Services;

public class CancelCheckRunPaymentsServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public static CancelCheckRunPaymentsServiceResult Success()
    {
        return new CancelCheckRunPaymentsServiceResult { IsSuccess = true };
    }

    public static CancelCheckRunPaymentsServiceResult Error(string errorMessage)
    {
        return new CancelCheckRunPaymentsServiceResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}