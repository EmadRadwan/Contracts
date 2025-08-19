namespace Domain;

public class ProductStoreGroup
{
    public ProductStoreGroup()
    {
        InversePrimaryParentGroup = new HashSet<ProductStoreGroup>();
        ProductPrices = new HashSet<ProductPrice>();
        ProductStoreGroupMembers = new HashSet<ProductStoreGroupMember>();
        ProductStoreGroupRoles = new HashSet<ProductStoreGroupRole>();
        ProductStoreGroupRollupParentGroups = new HashSet<ProductStoreGroupRollup>();
        ProductStoreGroupRollupProductStoreGroups = new HashSet<ProductStoreGroupRollup>();
        ProductStores = new HashSet<ProductStore>();
        VendorProducts = new HashSet<VendorProduct>();
    }

    public string ProductStoreGroupId { get; set; } = null!;
    public string? ProductStoreGroupTypeId { get; set; }
    public string? PrimaryParentGroupId { get; set; }
    public string? ProductStoreGroupName { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStoreGroup? PrimaryParentGroup { get; set; }
    public ProductStoreGroupType? ProductStoreGroupType { get; set; }
    public ICollection<ProductStoreGroup> InversePrimaryParentGroup { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
    public ICollection<ProductStoreGroupMember> ProductStoreGroupMembers { get; set; }
    public ICollection<ProductStoreGroupRole> ProductStoreGroupRoles { get; set; }
    public ICollection<ProductStoreGroupRollup> ProductStoreGroupRollupParentGroups { get; set; }
    public ICollection<ProductStoreGroupRollup> ProductStoreGroupRollupProductStoreGroups { get; set; }
    public ICollection<ProductStore> ProductStores { get; set; }
    public ICollection<VendorProduct> VendorProducts { get; set; }
}