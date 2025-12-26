namespace Application.Order.SalesRequests;

public class SalesRequestInstallmentDto
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsAdvance { get; set; }
}