namespace Domain;

public class TimeEntry
{
    public string TimeEntryId { get; set; } = null!;
    public string? PartyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? RateTypeId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? TimesheetId { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public double? Hours { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem? InvoiceI { get; set; }
    public Party? Party { get; set; }
    public RateType? RateType { get; set; }
    public Timesheet? Timesheet { get; set; }
    public WorkEffort? WorkEffort { get; set; }
}