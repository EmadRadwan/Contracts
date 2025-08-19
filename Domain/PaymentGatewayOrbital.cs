namespace Domain;

public class PaymentGatewayOrbital
{
    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? Username { get; set; }
    public string? ConnectionPassword { get; set; }
    public string? MerchantId { get; set; }
    public string? EngineClass { get; set; }
    public string? HostName { get; set; }
    public int? Port { get; set; }
    public string? HostNameFailover { get; set; }
    public int? PortFailover { get; set; }
    public int? ConnectionTimeoutSeconds { get; set; }
    public int? ReadTimeoutSeconds { get; set; }
    public string? AuthorizationURI { get; set; }
    public string? SdkVersion { get; set; }
    public string? SslSocketFactory { get; set; }
    public string? ResponseType { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfig PaymentGatewayConfig { get; set; } = null!;
}