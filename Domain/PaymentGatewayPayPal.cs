namespace Domain;

public class PaymentGatewayPayPal
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? BusinessEmail { get; set; }
    public string? ApiUserName { get; set; }
    public string? ApiPassword { get; set; }
    public string? ApiSignature { get; set; }
    public string? ApiEnvironment { get; set; }
    public string? NotifyUrl { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelReturnUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ConfirmTemplate { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ConfirmUrl { get; set; }
    public string? ShippingCallbackUrl { get; set; }
    public string? RequireConfirmedShipping { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}