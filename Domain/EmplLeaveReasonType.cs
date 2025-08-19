namespace Domain;

public class EmplLeaveReasonType
{
    public EmplLeaveReasonType()
    {
        EmplLeaves = new HashSet<EmplLeave>();
        InverseParentType = new HashSet<EmplLeaveReasonType>();
    }

    public string EmplLeaveReasonTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplLeaveReasonType? ParentType { get; set; }
    public ICollection<EmplLeave> EmplLeaves { get; set; }
    public ICollection<EmplLeaveReasonType> InverseParentType { get; set; }
}