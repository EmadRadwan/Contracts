namespace Domain;

public class PaymentGatewayPayflowPro
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? CertsPath { get; set; }
    public string? HostAddress { get; set; }
    public int? HostPort { get; set; }
    public int? Timeout { get; set; }
    public string? ProxyAddress { get; set; }
    public int? ProxyPort { get; set; }
    public string? ProxyLogon { get; set; }
    public string? ProxyPassword { get; set; }
    public string? Vendor { get; set; }
    public string? UserId { get; set; }
    public string? Pwd { get; set; }
    public string? Partner { get; set; }
    public string? CheckAvs { get; set; }
    public string? CheckCvv2 { get; set; }
    public string? PreAuth { get; set; }
    public string? EnableTransmit { get; set; }
    public string? LogFileName { get; set; }
    public int? LoggingLevel { get; set; }
    public int? MaxLogFileSize { get; set; }
    public string? StackTraceOn { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ReturnUrl { get; set; }
    public string? CancelReturnUrl { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}