namespace Domain;

public class PaymentGatewayRespMsg
{
    public string PaymentGatewayRespMsgId { get; set; } = null!;
    public string? PaymentGatewayResponseId { get; set; }
    public string? PgrMessage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayResponse? PaymentGatewayResponse { get; set; }
}