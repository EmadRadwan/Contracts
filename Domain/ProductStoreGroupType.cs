namespace Domain;

public class ProductStoreGroupType
{
    public ProductStoreGroupType()
    {
        ProductStoreGroups = new HashSet<ProductStoreGroup>();
    }

    public string ProductStoreGroupTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductStoreGroup> ProductStoreGroups { get; set; }
}