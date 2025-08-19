namespace Domain;

public class SegmentGroupRole
{
    public string SegmentGroupId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRole PartyRole { get; set; } = null!;
    public SegmentGroup SegmentGroup { get; set; } = null!;
}