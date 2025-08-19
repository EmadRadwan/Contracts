namespace Domain;

public class EmplLeave
{
    public string PartyId { get; set; } = null!;
    public string LeaveTypeId { get; set; } = null!;
    public string? EmplLeaveReasonTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? ApproverPartyId { get; set; }
    public string? LeaveStatus { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? ApproverParty { get; set; }
    public EmplLeaveReasonType? EmplLeaveReasonType { get; set; }
    public StatusItem? LeaveStatusNavigation { get; set; }
    public EmplLeaveType LeaveType { get; set; } = null!;
    public Party Party { get; set; } = null!;
}