using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderType
{
    public OrderType()
    {
        InverseParentType = new HashSet<OrderType>();
        OrderHeaders = new HashSet<OrderHeader>();
        OrderTypeAttrs = new HashSet<OrderTypeAttr>();
        PartyPrefDocTypeTpls = new HashSet<PartyPrefDocTypeTpl>();
    }

    public string OrderTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderType? ParentType { get; set; }
    public ICollection<OrderType> InverseParentType { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<OrderTypeAttr> OrderTypeAttrs { get; set; }
    public ICollection<PartyPrefDocTypeTpl> PartyPrefDocTypeTpls { get; set; }
}