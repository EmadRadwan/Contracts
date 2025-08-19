namespace Domain;

public class ProductConfig
{
    public string ProductId { get; set; } = null!;
    public string ConfigItemId { get; set; } = null!;
    public int SequenceNum { get; set; }
    public DateTime FromDate { get; set; }
    public string? Description { get; set; }
    public string? LongDescription { get; set; }
    public string? ConfigTypeId { get; set; }
    public string? DefaultConfigOptionId { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsMandatory { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigItem ConfigItem { get; set; } = null!;
    public Product Product { get; set; } = null!;
}