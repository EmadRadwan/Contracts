using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductAssocType
{
    public ProductAssocType()
    {
        InverseParentType = new HashSet<ProductAssocType>();
        ProductAssocs = new HashSet<ProductAssoc>();
    }

    public string ProductAssocTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductAssocType? ParentType { get; set; }
    public ICollection<ProductAssocType> InverseParentType { get; set; }
    public ICollection<ProductAssoc> ProductAssocs { get; set; }
}