using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class InventoryItemType
{
    public InventoryItemType()
    {
        Facilities = new HashSet<Facility>();
        InventoryItemTypeAttrs = new HashSet<InventoryItemTypeAttr>();
        InventoryItems = new HashSet<InventoryItem>();
        InverseParentType = new HashSet<InventoryItemType>();
        Products = new HashSet<Product>();
    }

    public string InventoryItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InventoryItemType? ParentType { get; set; }
    public ICollection<Facility> Facilities { get; set; }
    public ICollection<InventoryItemTypeAttr> InventoryItemTypeAttrs { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; }
    public ICollection<InventoryItemType> InverseParentType { get; set; }
    public ICollection<Product> Products { get; set; }
}