namespace Domain;

public class PerfRatingType
{
    public PerfRatingType()
    {
        InverseParentType = new HashSet<PerfRatingType>();
    }

    public string PerfRatingTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PerfRatingType? ParentType { get; set; }
    public ICollection<PerfRatingType> InverseParentType { get; set; }
}