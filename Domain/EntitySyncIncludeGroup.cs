namespace Domain;

public class EntitySyncIncludeGroup
{
    public string EntitySyncId { get; set; } = null!;
    public string EntityGroupId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EntityGroup EntityGroup { get; set; } = null!;
    public EntitySync EntitySync { get; set; } = null!;
}