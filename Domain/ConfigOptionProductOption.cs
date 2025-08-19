namespace Domain;

public class ConfigOptionProductOption
{
    public string ConfigId { get; set; } = null!;
    public string ConfigItemId { get; set; } = null!;
    public int SequenceNum { get; set; }
    public string ConfigOptionId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string? ProductOptionId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigConfig ProductConfigConfig { get; set; } = null!;
    public ProductConfigProduct ProductConfigProduct { get; set; } = null!;
}