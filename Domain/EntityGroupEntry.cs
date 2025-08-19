namespace Domain;

public class EntityGroupEntry
{
    public string EntityGroupId { get; set; } = null!;
    public string EntityOrPackage { get; set; } = null!;
    public string? ApplEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EntityGroup EntityGroup { get; set; } = null!;
}