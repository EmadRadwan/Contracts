namespace Domain;

public class WorkEffortFixedAssetStd
{
    public string WorkEffortId { get; set; } = null!;
    public string FixedAssetTypeId { get; set; } = null!;
    public double? EstimatedQuantity { get; set; }
    public double? EstimatedDuration { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetType FixedAssetType { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}