using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class OrderAdjustmentType
{
    public OrderAdjustmentType()
    {
        InverseParentType = new HashSet<OrderAdjustmentType>();
        OrderAdjustmentTypeAttrs = new HashSet<OrderAdjustmentTypeAttr>();
        OrderAdjustments = new HashSet<OrderAdjustment>();
        ProductPromoActions = new HashSet<ProductPromoAction>();
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
    }

    public string OrderAdjustmentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderAdjustmentType? ParentType { get; set; }
    public ICollection<OrderAdjustmentType> InverseParentType { get; set; }
    public ICollection<OrderAdjustmentTypeAttr> OrderAdjustmentTypeAttrs { get; set; }
    public ICollection<OrderAdjustment> OrderAdjustments { get; set; }
    public ICollection<ProductPromoAction> ProductPromoActions { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
}