namespace Domain;

public class PaymentGatewayEway
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? CustomerId { get; set; }
    public string? RefundPwd { get; set; }
    public string? TestMode { get; set; }
    public string? EnableCvn { get; set; }
    public string? EnableBeagle { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}