using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentGatewayConfig
{
    public PaymentGatewayConfig()
    {
        ProductStorePaymentSettings = new HashSet<ProductStorePaymentSetting>();
    }

    public string PaymentGatewayConfigId { get; set; } = null!;
    public string? PaymentGatewayConfigTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfigType? PaymentGatewayConfigType { get; set; }
    public PaymentGatewayAuthorizeNet PaymentGatewayAuthorizeNet { get; set; } = null!;
    public PaymentGatewayClearCommerce PaymentGatewayClearCommerce { get; set; } = null!;
    public PaymentGatewayCyberSource PaymentGatewayCyberSource { get; set; } = null!;
    public PaymentGatewayEway PaymentGatewayEway { get; set; } = null!;
    public PaymentGatewayOrbital PaymentGatewayOrbital { get; set; } = null!;
    public PaymentGatewayPayPal PaymentGatewayPayPal { get; set; } = null!;
    public PaymentGatewayPayflowPro PaymentGatewayPayflowPro { get; set; } = null!;
    public PaymentGatewaySagePay PaymentGatewaySagePay { get; set; } = null!;
    public PaymentGatewaySecurePay PaymentGatewaySecurePay { get; set; } = null!;
    public PaymentGatewayWorldPay PaymentGatewayWorldPay { get; set; } = null!;
    public ICollection<ProductStorePaymentSetting> ProductStorePaymentSettings { get; set; }
}