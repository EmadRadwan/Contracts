namespace Domain;

public class PaymentGatewayWorldPay
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? RedirectUrl { get; set; }
    public string? InstId { get; set; }
    public string? AuthMode { get; set; }
    public string? FixContact { get; set; }
    public string? HideContact { get; set; }
    public string? HideCurrency { get; set; }
    public string? LangId { get; set; }
    public string? NoLanguageMenu { get; set; }
    public string? WithDelivery { get; set; }
    public int? TestMode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}