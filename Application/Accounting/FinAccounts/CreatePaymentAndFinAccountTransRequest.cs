namespace Application.Accounting.FinAccounts;

public class CreatePaymentAndFinAccountTransRequest
{
    public string PaymentMethodId { get; set; }
    public string IsDepositWithDrawPayment { get; set; }
    public string FinAccountTransTypeId { get; set; }
    public string? PaymentTypeId { get; set; }
    public bool? IsBankTransfer { get; set; }

    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public decimal Amount { get; set; }
    public string? StatusId { get; set; }
    public string? SalesRequestId { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public string? Comments { get; set; }
    public string? PaymentRefNum { get; set; }
    public string? ChequeNumber { get; set; }
    public DateOnly? ChequeDate { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? ProjectId { get; set; }
    public string? CostCenterId { get; set; }

}