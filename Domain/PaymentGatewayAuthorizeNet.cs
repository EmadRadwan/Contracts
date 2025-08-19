namespace Domain;

public class PaymentGatewayAuthorizeNet
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? TransactionUrl { get; set; }
    public string? CertificateAlias { get; set; }
    public string? ApiVersion { get; set; }
    public string? DelimitedData { get; set; }
    public string? DelimiterChar { get; set; }
    public string? CpVersion { get; set; }
    public string? CpMarketType { get; set; }
    public string? CpDeviceType { get; set; }
    public string? Method { get; set; }
    public string? EmailCustomer { get; set; }
    public string? EmailMerchant { get; set; }
    public string? TestMode { get; set; }
    public string? RelayResponse { get; set; }
    public string? TranKey { get; set; }
    public string? UserId { get; set; }
    public string? Pwd { get; set; }
    public string? TransDescription { get; set; }
    public int? DuplicateWindow { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}