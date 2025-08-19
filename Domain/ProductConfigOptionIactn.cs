namespace Domain;

public class ProductConfigOptionIactn
{
    public string ConfigItemId { get; set; } = null!;
    public string ConfigOptionId { get; set; } = null!;
    public string ConfigItemIdTo { get; set; } = null!;
    public string ConfigOptionIdTo { get; set; } = null!;
    public int SequenceNum { get; set; }
    public string? ConfigIactnTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigOption Config { get; set; } = null!;
    public ProductConfigItem ConfigItem { get; set; } = null!;
    public ProductConfigItem ConfigItemIdToNavigation { get; set; } = null!;
    public ProductConfigOption ConfigNavigation { get; set; } = null!;
}