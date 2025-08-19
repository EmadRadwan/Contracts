namespace Domain;

public class ProductConfigItem
{
    public ProductConfigItem()
    {
        ProdConfItemContents = new HashSet<ProdConfItemContent>();
        ProductConfigConfigs = new HashSet<ProductConfigConfig>();
        ProductConfigOptionIactnConfigItemIdToNavigations = new HashSet<ProductConfigOptionIactn>();
        ProductConfigOptionIactnConfigItems = new HashSet<ProductConfigOptionIactn>();
        ProductConfigOptions = new HashSet<ProductConfigOption>();
        ProductConfigProducts = new HashSet<ProductConfigProduct>();
        ProductConfigs = new HashSet<ProductConfig>();
    }

    public string ConfigItemId { get; set; } = null!;
    public string? ConfigItemTypeId { get; set; }
    public string? ConfigItemName { get; set; }
    public string? Description { get; set; }
    public string? LongDescription { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProdConfItemContent> ProdConfItemContents { get; set; }
    public ICollection<ProductConfigConfig> ProductConfigConfigs { get; set; }
    public ICollection<ProductConfigOptionIactn> ProductConfigOptionIactnConfigItemIdToNavigations { get; set; }
    public ICollection<ProductConfigOptionIactn> ProductConfigOptionIactnConfigItems { get; set; }
    public ICollection<ProductConfigOption> ProductConfigOptions { get; set; }
    public ICollection<ProductConfigProduct> ProductConfigProducts { get; set; }
    public ICollection<ProductConfig> ProductConfigs { get; set; }
}