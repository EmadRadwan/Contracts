namespace Application.Accounting.BillingAccounts;

public class BillingAccountPaymentDto
{
    public string BillingAccountId { get; set; }
    public string PaymentId { get; set; }
    public string PaymentMethodTypeId { get; set; }
    public string InvoiceId { get; set; }
    public string InvoiceItemSeqId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal Amount { get; set; }
}