namespace Domain;

public class ProductMaint
{
    public string ProductId { get; set; } = null!;
    public string ProductMaintSeqId { get; set; } = null!;
    public string? ProductMaintTypeId { get; set; }
    public string? MaintName { get; set; }
    public string? MaintTemplateWorkEffortId { get; set; }
    public decimal? IntervalQuantity { get; set; }
    public string? IntervalUomId { get; set; }
    public string? IntervalMeterTypeId { get; set; }
    public int? RepeatCount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductMeterType? IntervalMeterType { get; set; }
    public Uom? IntervalUom { get; set; }
    public WorkEffort? MaintTemplateWorkEffort { get; set; }
    public Product Product { get; set; } = null!;
    public ProductMaintType? ProductMaintType { get; set; }
}