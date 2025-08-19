namespace Domain;

public class FixedAssetMeter
{
    public string FixedAssetId { get; set; } = null!;
    public string ProductMeterTypeId { get; set; } = null!;
    public DateTime ReadingDate { get; set; }
    public decimal? MeterValue { get; set; }
    public string? ReadingReasonEnumId { get; set; }
    public string? MaintHistSeqId { get; set; }
    public string? WorkEffortId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetMaint? FixedAssetMaint { get; set; }
    public ProductMeterType ProductMeterType { get; set; } = null!;
}