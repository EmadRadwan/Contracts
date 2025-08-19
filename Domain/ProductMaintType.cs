namespace Domain;

public class ProductMaintType
{
    public ProductMaintType()
    {
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        InverseParentType = new HashSet<ProductMaintType>();
        ProductMaints = new HashSet<ProductMaint>();
    }

    public string ProductMaintTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? ParentTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductMaintType? ParentType { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<ProductMaintType> InverseParentType { get; set; }
    public ICollection<ProductMaint> ProductMaints { get; set; }
}