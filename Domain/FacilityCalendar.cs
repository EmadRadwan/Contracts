namespace Domain;

public class FacilityCalendar
{
    public string FacilityId { get; set; } = null!;
    public string CalendarId { get; set; } = null!;
    public string FacilityCalendarTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public FacilityCalendarType FacilityCalendarType { get; set; } = null!;
}