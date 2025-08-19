namespace Application.Accounting.Payments;


public class CreatePaymentParam
{
    public string? PaymentId { get; set; }
    public string? PaymentTypeId { get; set; }
    public string? ActualCurrencyUomId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public string? StatusId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? PaymentPreferenceId { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? ActualCurrencyAmount { get; set; }
    public string? PaymentRefNum { get; set; }
    public string? Comments { get; set; }
}