using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductStorePaymentSetting
{
    public string ProductStoreId { get; set; } = null!;
    public string PaymentMethodTypeId { get; set; } = null!;
    public string PaymentServiceTypeEnumId { get; set; } = null!;
    public string? PaymentService { get; set; }
    public string? PaymentCustomMethodId { get; set; }
    public string? PaymentGatewayConfigId { get; set; }
    public string? PaymentPropertiesPath { get; set; }
    public string? ApplyToAllProducts { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? PaymentCustomMethod { get; set; }
    public PaymentGatewayConfig? PaymentGatewayConfig { get; set; }
    public PaymentMethodType PaymentMethodType { get; set; } = null!;
    public Enumeration PaymentServiceTypeEnum { get; set; } = null!;
    public ProductStore ProductStore { get; set; } = null!;
}