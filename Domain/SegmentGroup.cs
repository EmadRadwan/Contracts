namespace Domain;

public class SegmentGroup
{
    public SegmentGroup()
    {
        SegmentGroupClassifications = new HashSet<SegmentGroupClassification>();
        SegmentGroupGeos = new HashSet<SegmentGroupGeo>();
        SegmentGroupRoles = new HashSet<SegmentGroupRole>();
    }

    public string SegmentGroupId { get; set; } = null!;
    public string? SegmentGroupTypeId { get; set; }
    public string? Description { get; set; }
    public string? ProductStoreId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStore? ProductStore { get; set; }
    public SegmentGroupType? SegmentGroupType { get; set; }
    public ICollection<SegmentGroupClassification> SegmentGroupClassifications { get; set; }
    public ICollection<SegmentGroupGeo> SegmentGroupGeos { get; set; }
    public ICollection<SegmentGroupRole> SegmentGroupRoles { get; set; }
}