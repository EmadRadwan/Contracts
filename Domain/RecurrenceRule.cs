namespace Domain;

public class RecurrenceRule
{
    public RecurrenceRule()
    {
        RecurrenceInfoExceptionRules = new HashSet<RecurrenceInfo>();
        RecurrenceInfoRecurrenceRules = new HashSet<RecurrenceInfo>();
    }

    public string RecurrenceRuleId { get; set; } = null!;
    public string? Frequency { get; set; }
    public DateTime? UntilDateTime { get; set; }
    public int? CountNumber { get; set; }
    public int? IntervalNumber { get; set; }
    public string? BySecondList { get; set; }
    public string? ByMinuteList { get; set; }
    public string? ByHourList { get; set; }
    public string? ByDayList { get; set; }
    public string? ByMonthDayList { get; set; }
    public string? ByYearDayList { get; set; }
    public string? ByWeekNoList { get; set; }
    public string? ByMonthList { get; set; }
    public string? BySetPosList { get; set; }
    public string? WeekStart { get; set; }
    public string? XName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<RecurrenceInfo> RecurrenceInfoExceptionRules { get; set; }
    public ICollection<RecurrenceInfo> RecurrenceInfoRecurrenceRules { get; set; }
}