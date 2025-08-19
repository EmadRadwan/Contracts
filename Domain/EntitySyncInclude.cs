namespace Domain;

public class EntitySyncInclude
{
    public string EntitySyncId { get; set; } = null!;
    public string EntityOrPackage { get; set; } = null!;
    public string? ApplEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EntitySync EntitySync { get; set; } = null!;
}