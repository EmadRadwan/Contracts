namespace Domain;

public class SalesRequestInstallment
{
    public string SalesRequestId { get; set; } = null!;

    public int InstallmentNumber { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal Amount { get; set; }
    public bool IsAdvance { get; set; }

    public SalesRequest SalesRequest { get; set; } = null!;
}