namespace Domain;

public class PaymentGatewayCyberSource
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? MerchantId { get; set; }
    public string? ApiVersion { get; set; }
    public string? Production { get; set; }
    public string? KeysDir { get; set; }
    public string? KeysFile { get; set; }
    public string? LogEnabled { get; set; }
    public string? LogDir { get; set; }
    public string? LogFile { get; set; }
    public int? LogSize { get; set; }
    public string? MerchantDescr { get; set; }
    public string? MerchantContact { get; set; }
    public string? AutoBill { get; set; }
    public string? EnableDav { get; set; }
    public string? FraudScore { get; set; }
    public string? IgnoreAvs { get; set; }
    public string? DisableBillAvs { get; set; }
    public string? AvsDeclineCodes { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}