namespace Domain;

public class EntitySync
{
    public EntitySync()
    {
        EntitySyncHistories = new HashSet<EntitySyncHistory>();
        EntitySyncIncludeGroups = new HashSet<EntitySyncIncludeGroup>();
        EntitySyncIncludes = new HashSet<EntitySyncInclude>();
    }

    public string EntitySyncId { get; set; } = null!;
    public string? RunStatusId { get; set; }
    public DateTime? LastSuccessfulSynchTime { get; set; }
    public DateTime? LastHistoryStartDate { get; set; }
    public DateTime? PreOfflineSynchTime { get; set; }
    public int? OfflineSyncSplitMillis { get; set; }
    public int? SyncSplitMillis { get; set; }
    public int? SyncEndBufferMillis { get; set; }
    public int? MaxRunningNoUpdateMillis { get; set; }
    public string? TargetServiceName { get; set; }
    public string? TargetDelegatorName { get; set; }
    public double? KeepRemoveInfoHours { get; set; }
    public string? ForPullOnly { get; set; }
    public string? ForPushOnly { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<EntitySyncHistory> EntitySyncHistories { get; set; }
    public ICollection<EntitySyncIncludeGroup> EntitySyncIncludeGroups { get; set; }
    public ICollection<EntitySyncInclude> EntitySyncIncludes { get; set; }
}