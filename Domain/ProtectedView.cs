namespace Domain;

public class ProtectedView
{
    public string GroupId { get; set; } = null!;
    public string ViewNameId { get; set; } = null!;
    public int? MaxHits { get; set; }
    public int? MaxHitsDuration { get; set; }
    public int? TarpitDuration { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SecurityGroup Group { get; set; } = null!;
}