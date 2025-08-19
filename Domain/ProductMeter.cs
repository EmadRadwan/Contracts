namespace Domain;

public class ProductMeter
{
    public string ProductId { get; set; } = null!;
    public string ProductMeterTypeId { get; set; } = null!;
    public string? MeterUomId { get; set; }
    public string? MeterName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? MeterUom { get; set; }
    public Product Product { get; set; } = null!;
    public ProductMeterType ProductMeterType { get; set; } = null!;
}