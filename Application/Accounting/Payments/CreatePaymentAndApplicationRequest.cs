namespace Application.Accounting.Payments;

public class CreatePaymentAndApplicationRequest
{
    public string PaymentTypeId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string? StatusId { get; set; }
    public decimal Amount { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public string BillingAccountId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? TaxAuthGeoId { get; set; }
}
