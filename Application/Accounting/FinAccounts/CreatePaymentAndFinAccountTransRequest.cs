namespace Application.Accounting.FinAccounts;

public class CreatePaymentAndFinAccountTransRequest
{
    public string PaymentMethodId { get; set; }
    public string IsDepositWithDrawPayment { get; set; }
    public string FinAccountTransTypeId { get; set; }
    public string? PaymentTypeId { get; set; }

    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public decimal Amount { get; set; }
    public string StatusId { get; set; }
    public DateTime? PaymentDate { get; set; }
}