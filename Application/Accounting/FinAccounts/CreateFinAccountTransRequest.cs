namespace Application.Accounting.FinAccounts;

public class CreateFinAccountTransRequest
{
    public string FinAccountId { get; set; }
    public string GlAccountId { get; set; }
    public string? PaymentId { get; set; }
    public string FinAccountTransTypeId { get; set; }
    public DateOnly? TransactionDate { get; set; }
    public DateOnly? EntryDate { get; set; }
    public string StatusId { get; set; }
    public string PerformedByPartyId { get; set; }
    public decimal Amount { get; set; }
}