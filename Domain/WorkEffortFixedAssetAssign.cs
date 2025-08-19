namespace Domain;

public class WorkEffortFixedAssetAssign
{
    public string WorkEffortId { get; set; } = null!;
    public string FixedAssetId { get; set; } = null!;
    public string? StatusId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AvailabilityStatusId { get; set; }
    public decimal? AllocatedCost { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem? AvailabilityStatus { get; set; }
    public FixedAsset FixedAsset { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public WorkEffort WorkEffort { get; set; } = null!;
}