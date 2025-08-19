namespace Application.Accounting.FinAccounts;

public class CreateFinAccountTransRequest
{
    public string FinAccountId { get; set; }
    public string GlAccountId { get; set; }
    public string? PaymentId { get; set; }
    public string FinAccountTransTypeId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime? EntryDate { get; set; }
    public string StatusId { get; set; }
    public string PerformedByPartyId { get; set; }
    public decimal Amount { get; set; }
}