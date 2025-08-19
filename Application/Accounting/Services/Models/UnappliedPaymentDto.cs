namespace Application.Accounting.Services.Models;

public class UnappliedPaymentDto
{
    public string PaymentId { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string PaymentTypeId { get; set; }
    public string PaymentTypeDescription { get; set; }
    public decimal Amount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string CurrencyUomId { get; set; }
    public string PaymentParentTypeId { get; set; }
}