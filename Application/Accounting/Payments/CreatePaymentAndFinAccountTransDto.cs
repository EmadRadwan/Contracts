namespace Application.Accounting.Payments;


public class CreatePaymentAndFinAccountTransDto
{
    public string? PaymentId { get; set; }
    public string? PaymentTypeId { get; set; }
    public string? PaymentMethodId { get; set; }
    public string? FinAccountId { get; set; }
    public string? PaymentMethodTypeId { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public decimal? Amount { get; set; }
    public string? Comments { get; set; }
    public string? FinAccountTransId { get; set; }
    public DateTime? EffectiveDate { get; set; }
}