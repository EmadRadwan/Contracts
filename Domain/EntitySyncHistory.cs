namespace Domain;

public class EntitySyncHistory
{
    public string EntitySyncId { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public string? RunStatusId { get; set; }
    public DateTime? BeginningSynchTime { get; set; }
    public DateTime? LastSuccessfulSynchTime { get; set; }
    public DateTime? LastCandidateEndTime { get; set; }
    public int? LastSplitStartTime { get; set; }
    public int? ToCreateInserted { get; set; }
    public int? ToCreateUpdated { get; set; }
    public int? ToCreateNotUpdated { get; set; }
    public int? ToStoreInserted { get; set; }
    public int? ToStoreUpdated { get; set; }
    public int? ToStoreNotUpdated { get; set; }
    public int? ToRemoveDeleted { get; set; }
    public int? ToRemoveAlreadyDeleted { get; set; }
    public int? TotalRowsExported { get; set; }
    public int? TotalRowsToCreate { get; set; }
    public int? TotalRowsToStore { get; set; }
    public int? TotalRowsToRemove { get; set; }
    public int? TotalSplits { get; set; }
    public int? TotalStoreCalls { get; set; }
    public int? RunningTimeMillis { get; set; }
    public int? PerSplitMinMillis { get; set; }
    public int? PerSplitMaxMillis { get; set; }
    public int? PerSplitMinItems { get; set; }
    public int? PerSplitMaxItems { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EntitySync EntitySync { get; set; } = null!;
}