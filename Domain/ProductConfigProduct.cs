namespace Domain;

public class ProductConfigProduct
{
    public ProductConfigProduct()
    {
        ConfigOptionProductOptions = new HashSet<ConfigOptionProductOption>();
    }

    public string ConfigItemId { get; set; } = null!;
    public string ConfigOptionId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductConfigOption Config { get; set; } = null!;
    public ProductConfigItem ConfigItem { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<ConfigOptionProductOption> ConfigOptionProductOptions { get; set; }
}