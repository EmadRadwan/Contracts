namespace Application.Accounting.FinAccounts;

public class CreatePaymentAndFinAccountTransResponse
{
    public string PaymentId { get; set; }
    public string FinAccountTransId { get; set; }
    public string CurrencyUomId { get; set; }
    public string ActualCurrencyUomId { get; set; }
    public string? Comments { get; set; }
    public string? ChequeNumber { get; set; }
    public DateOnly? ChequeDate { get; set; }
    public string? PartyIdFromName { get; set; }
    public string? PartyIdToName { get; set; }
}
