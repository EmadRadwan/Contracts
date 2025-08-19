namespace Domain;

public class PaymentGatewaySecurePay
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? Pwd { get; set; }
    public string? ServerURL { get; set; }
    public int? ProcessTimeout { get; set; }
    public string? EnableAmountRound { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}