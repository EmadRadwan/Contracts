namespace Application.Order.Orders;

public class ResetGrandTotalResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public static ResetGrandTotalResult Success() => new ResetGrandTotalResult { IsSuccess = true };
    public static ResetGrandTotalResult Failure(string errorMessage) => new ResetGrandTotalResult { IsSuccess = false, ErrorMessage = errorMessage };
}
