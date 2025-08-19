namespace Domain;

public class PaymentGatewaySagePay
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? Vendor { get; set; }
    public string? ProductionHost { get; set; }
    public string? TestingHost { get; set; }
    public string? SagePayMode { get; set; }
    public string? ProtocolVersion { get; set; }
    public string? AuthenticationTransType { get; set; }
    public string? AuthenticationUrl { get; set; }
    public string? AuthoriseTransType { get; set; }
    public string? AuthoriseUrl { get; set; }
    public string? ReleaseTransType { get; set; }
    public string? ReleaseUrl { get; set; }
    public string? VoidUrl { get; set; }
    public string? RefundUrl { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}