using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductType
{
    public ProductType()
    {
        InverseParentType = new HashSet<ProductType>();
        ProductTypeAttrs = new HashSet<ProductTypeAttr>();
        Products = new HashSet<Product>();
    }

    public string ProductTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? IsPhysical { get; set; }
    public string? IsDigital { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }


    public ProductType? ParentType { get; set; }
    public ICollection<ProductType> InverseParentType { get; set; }
    public ICollection<ProductTypeAttr> ProductTypeAttrs { get; set; }
    public ICollection<Product> Products { get; set; }
}