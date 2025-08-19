namespace Domain;

public class PerfReviewItemType
{
    public PerfReviewItemType()
    {
        InverseParentType = new HashSet<PerfReviewItemType>();
    }

    public string PerfReviewItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PerfReviewItemType? ParentType { get; set; }
    public ICollection<PerfReviewItemType> InverseParentType { get; set; }
}