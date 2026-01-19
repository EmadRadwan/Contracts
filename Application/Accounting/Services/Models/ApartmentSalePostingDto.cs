namespace Application.Accounting.Services.Models;

public class ApartmentSalePostingDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionTypeId { get; set; } = string.Empty;
    public DateTime? TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GlAccountId { get; set; } = string.Empty;
    public string? GlAccountTypeId { get; set; }
    public decimal Amount { get; set; }
    public string DebitCreditFlag { get; set; } = string.Empty;
    public string CurrencyUomId { get; set; } = string.Empty;
    public string? SalesRequestId { get; set; }
    public decimal ImpactOnBalance { get; set; }
}