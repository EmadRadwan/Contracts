namespace Application.Accounting.Payments;


public class PaymentDto
{
    public string? PaymentId { get; set; }
    public string? PaymentTypeId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PaymentTypeDescription { get; set; }

    public string? PartyIdFrom { get; set; }
    public string? PartyIdFromName { get; set; }
    public string? PartyIdTo { get; set; }
    public string? PartyIdToName { get; set; }
    public string? RoleTypeIdTo { get; set; }
    public string? StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public bool IsPaymentDeleted { get; set; }
    public bool IsIncomingPayment { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentRefNum { get; set; }
    public decimal Amount { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? Comments { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public decimal? ActualCurrencyAmount { get; set; }
    public string? ActualCurrencyUomId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? LovText { get; set; }


    public decimal? Total { get; set; }
    public decimal? OutstandingAmount { get; set; }
}