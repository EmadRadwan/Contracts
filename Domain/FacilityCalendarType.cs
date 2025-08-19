namespace Domain;

public class FacilityCalendarType
{
    public FacilityCalendarType()
    {
        FacilityCalendars = new HashSet<FacilityCalendar>();
    }

    public string FacilityCalendarTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<FacilityCalendar> FacilityCalendars { get; set; }
}