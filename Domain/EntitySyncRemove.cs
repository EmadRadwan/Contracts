namespace Domain;

public class EntitySyncRemove
{
    public string EntitySyncRemoveId { get; set; } = null!;
    public string? PrimaryKeyRemoved { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}