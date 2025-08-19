using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderItemType
{
    public OrderItemType()
    {
        InverseParentType = new HashSet<OrderItemType>();
        OrderItemTypeAttrs = new HashSet<OrderItemTypeAttr>();
        OrderItems = new HashSet<OrderItem>();
    }

    public string OrderItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderItemType? ParentType { get; set; }
    public ICollection<OrderItemType> InverseParentType { get; set; }
    public ICollection<OrderItemTypeAttr> OrderItemTypeAttrs { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}