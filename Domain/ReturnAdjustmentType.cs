namespace Domain;

public class ReturnAdjustmentType
{
    public ReturnAdjustmentType()
    {
        InverseParentType = new HashSet<ReturnAdjustmentType>();
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
    }

    public string ReturnAdjustmentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ReturnAdjustmentType? ParentType { get; set; }
    public ICollection<ReturnAdjustmentType> InverseParentType { get; set; }
    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
}