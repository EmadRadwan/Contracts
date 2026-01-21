namespace Application.Accounting.Services.Models;

public class ChequeIssuedPostingDto
{
    public string TransactionId { get; set; }
    public string TransactionTypeId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string Description { get; set; }
    public string GlAccountId { get; set; }
    public string GlAccountTypeId { get; set; }
    public decimal Amount { get; set; }
    public string DebitCreditFlag { get; set; }
    public string CurrencyUomId { get; set; }
    public decimal ImpactOnBalance { get; set; }     // Positive = increases what customer owes
}