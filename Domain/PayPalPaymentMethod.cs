namespace Domain;

public class PayPalPaymentMethod
{
    public string PaymentMethodId { get; set; } = null!;
    public string? PayerId { get; set; }
    public string? ExpressCheckoutToken { get; set; }
    public string? PayerStatus { get; set; }
    public string? AvsAddr { get; set; }
    public string? AvsZip { get; set; }
    public string? CorrelationId { get; set; }
    public string? ContactMechId { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public PostalAddress? ContactMechNavigation { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = null!;
}