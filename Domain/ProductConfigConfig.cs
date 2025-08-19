namespace Domain;

public class ProductConfigConfig
{
    public ProductConfigConfig()
    {
        ConfigOptionProductOptions = new HashSet<ConfigOptionProductOption>();
    }

    public string ConfigId { get; set; } = null!;
    public string ConfigItemId { get; set; } = null!;
    public int SequenceNum { get; set; }
    public string ConfigOptionId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigOption Config { get; set; } = null!;
    public ProductConfigItem ConfigItem { get; set; } = null!;
    public ICollection<ConfigOptionProductOption> ConfigOptionProductOptions { get; set; }
}