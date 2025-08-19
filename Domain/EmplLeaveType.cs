namespace Domain;

public class EmplLeaveType
{
    public EmplLeaveType()
    {
        EmplLeaves = new HashSet<EmplLeave>();
        InverseParentType = new HashSet<EmplLeaveType>();
    }

    public string LeaveTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplLeaveType? ParentType { get; set; }
    public ICollection<EmplLeave> EmplLeaves { get; set; }
    public ICollection<EmplLeaveType> InverseParentType { get; set; }
}