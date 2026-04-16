namespace Application.Order.SalesRequests;

public class SalesRequestInstallmentDto
{
    public int InstallmentNumber { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsAdvance { get; set; }
}