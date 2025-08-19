namespace Domain;

public class ProductConfigOption
{
    public ProductConfigOption()
    {
        ProductConfigConfigs = new HashSet<ProductConfigConfig>();
        ProductConfigOptionIactnConfigNavigations = new HashSet<ProductConfigOptionIactn>();
        ProductConfigOptionIactnConfigs = new HashSet<ProductConfigOptionIactn>();
        ProductConfigProducts = new HashSet<ProductConfigProduct>();
    }

    public string ConfigItemId { get; set; } = null!;
    public string ConfigOptionId { get; set; } = null!;
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? ConfigOptionName { get; set; }
    public string? Description { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigItem ConfigItem { get; set; } = null!;
    public ICollection<ProductConfigConfig> ProductConfigConfigs { get; set; }
    public ICollection<ProductConfigOptionIactn> ProductConfigOptionIactnConfigNavigations { get; set; }
    public ICollection<ProductConfigOptionIactn> ProductConfigOptionIactnConfigs { get; set; }
    public ICollection<ProductConfigProduct> ProductConfigProducts { get; set; }
}