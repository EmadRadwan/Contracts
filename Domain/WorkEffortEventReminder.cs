namespace Domain;

public class WorkEffortEventReminder
{
    public string WorkEffortId { get; set; } = null!;
    public string SequenceId { get; set; } = null!;
    public string? ContactMechId { get; set; }
    public string? PartyId { get; set; }
    public DateTime? ReminderDateTime { get; set; }
    public int? RepeatCount { get; set; }
    public int? RepeatInterval { get; set; }
    public int? CurrentCount { get; set; }
    public int? ReminderOffset { get; set; }
    public string? LocaleId { get; set; }
    public string? TimeZoneId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public Party? Party { get; set; }
    public WorkEffort WorkEffort { get; set; } = null!;
}