namespace Domain;

public class SegmentGroupGeo
{
    public string SegmentGroupId { get; set; } = null!;
    public string GeoId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo Geo { get; set; } = null!;
    public SegmentGroup SegmentGroup { get; set; } = null!;
}