namespace Domain;

public class Timesheet
{
    public Timesheet()
    {
        TimeEntries = new HashSet<TimeEntry>();
        TimesheetRoles = new HashSet<TimesheetRole>();
    }

    public string TimesheetId { get; set; } = null!;
    public string? PartyId { get; set; }
    public string? ClientPartyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public string? ApprovedByUserLoginId { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ApprovedByUserLogin { get; set; }
    public Party? ClientParty { get; set; }
    public Party? Party { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; }
    public ICollection<TimesheetRole> TimesheetRoles { get; set; }
}