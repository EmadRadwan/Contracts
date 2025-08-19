using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentGatewayConfigType
{
    public PaymentGatewayConfigType()
    {
        InverseParentType = new HashSet<PaymentGatewayConfigType>();
        PaymentGatewayConfigs = new HashSet<PaymentGatewayConfig>();
    }

    public string PaymentGatewayConfigTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGatewayConfigType? ParentType { get; set; }
    public ICollection<PaymentGatewayConfigType> InverseParentType { get; set; }
    public ICollection<PaymentGatewayConfig> PaymentGatewayConfigs { get; set; }
}