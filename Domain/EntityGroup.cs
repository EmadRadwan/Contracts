namespace Domain;

public class EntityGroup
{
    public EntityGroup()
    {
        EntityGroupEntries = new HashSet<EntityGroupEntry>();
        EntitySyncIncludeGroups = new HashSet<EntitySyncIncludeGroup>();
    }

    public string EntityGroupId { get; set; } = null!;
    public string? EntityGroupName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<EntityGroupEntry> EntityGroupEntries { get; set; }
    public ICollection<EntitySyncIncludeGroup> EntitySyncIncludeGroups { get; set; }
}