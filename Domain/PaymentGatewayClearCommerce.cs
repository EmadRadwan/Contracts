namespace Domain;

public class PaymentGatewayClearCommerce
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? SourceId { get; set; }
    public string? GroupId { get; set; }
    public string? ClientId { get; set; }
    public string? Username { get; set; }
    public string? Pwd { get; set; }
    public string? UserAlias { get; set; }
    public string? EffectiveAlias { get; set; }
    public string? ProcessMode { get; set; }
    public string? ServerURL { get; set; }
    public string? EnableCVM { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}