namespace Domain;

public class SegmentGroupClassification
{
    public string SegmentGroupId { get; set; } = null!;
    public string PartyClassificationGroupId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyClassificationGroup PartyClassificationGroup { get; set; } = null!;
    public SegmentGroup SegmentGroup { get; set; } = null!;
}