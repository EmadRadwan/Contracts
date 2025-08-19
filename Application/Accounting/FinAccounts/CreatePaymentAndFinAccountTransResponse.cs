namespace Application.Accounting.FinAccounts;

public class CreatePaymentAndFinAccountTransResponse
{
    public string PaymentId { get; set; }
    public string FinAccountTransId { get; set; }
    public string CurrencyUomId { get; set; }
    public string ActualCurrencyUomId { get; set; }
}
