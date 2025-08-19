namespace Domain;

public class PaymentApplication
{
    public string PaymentApplicationId { get; set; } = null!;
    public string? PaymentId { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public string? BillingAccountId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? ToPaymentId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public decimal? AmountApplied { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BillingAccount? BillingAccount { get; set; }
    public Invoice? Invoice { get; set; }
    public GlAccount? OverrideGlAccount { get; set; }
    public Payment? Payment { get; set; }
    public Geo? TaxAuthGeo { get; set; }
    public Payment? ToPayment { get; set; }
}