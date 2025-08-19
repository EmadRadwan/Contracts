namespace Domain;

public class SegmentGroupType
{
    public SegmentGroupType()
    {
        SegmentGroups = new HashSet<SegmentGroup>();
    }

    public string SegmentGroupTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<SegmentGroup> SegmentGroups { get; set; }
}