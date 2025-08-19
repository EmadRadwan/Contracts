namespace Domain;

public class ProductMeterType
{
    public ProductMeterType()
    {
        FixedAssetMaints = new HashSet<FixedAssetMaint>();
        FixedAssetMeters = new HashSet<FixedAssetMeter>();
        ProductMaints = new HashSet<ProductMaint>();
        ProductMeters = new HashSet<ProductMeter>();
    }

    public string ProductMeterTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DefaultUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? DefaultUom { get; set; }
    public ICollection<FixedAssetMaint> FixedAssetMaints { get; set; }
    public ICollection<FixedAssetMeter> FixedAssetMeters { get; set; }
    public ICollection<ProductMaint> ProductMaints { get; set; }
    public ICollection<ProductMeter> ProductMeters { get; set; }
}