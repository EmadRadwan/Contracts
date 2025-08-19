using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductCategoryType
{
    public ProductCategoryType()
    {
        InverseParentType = new HashSet<ProductCategoryType>();
        ProductCategories = new HashSet<ProductCategory>();
        ProductCategoryTypeAttrs = new HashSet<ProductCategoryTypeAttr>();
    }

    public string ProductCategoryTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategoryType? ParentType { get; set; }
    public ICollection<ProductCategoryType> InverseParentType { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
    public ICollection<ProductCategoryTypeAttr> ProductCategoryTypeAttrs { get; set; }
}