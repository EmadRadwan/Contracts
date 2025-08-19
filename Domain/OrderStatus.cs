using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderStatus
{
    public string OrderStatusId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? OrderPaymentPreferenceId { get; set; }
    public DateTime? StatusDatetime { get; set; }
    public string? StatusUserLogin { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader? Order { get; set; }
    public StatusItem? Status { get; set; }
    public UserLogin? StatusUserLoginNavigation { get; set; }
}