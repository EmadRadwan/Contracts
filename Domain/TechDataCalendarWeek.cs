using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class TechDataCalendarWeek
{
    public TechDataCalendarWeek()
    {
        TechDataCalendarExcWeeks = new HashSet<TechDataCalendarExcWeek>();
        TechDataCalendars = new HashSet<TechDataCalendar>();
    }

    public string CalendarWeekId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? MondayStartTime { get; set; }
    public double? MondayCapacity { get; set; }
    public DateTime? TuesdayStartTime { get; set; }
    public double? TuesdayCapacity { get; set; }
    public DateTime? WednesdayStartTime { get; set; }
    public double? WednesdayCapacity { get; set; }
    public DateTime? ThursdayStartTime { get; set; }
    public double? ThursdayCapacity { get; set; }
    public DateTime? FridayStartTime { get; set; }
    public double? FridayCapacity { get; set; }
    public DateTime? SaturdayStartTime { get; set; }
    public double? SaturdayCapacity { get; set; }
    public DateTime? SundayStartTime { get; set; }
    public double? SundayCapacity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<TechDataCalendarExcWeek> TechDataCalendarExcWeeks { get; set; }
    public ICollection<TechDataCalendar> TechDataCalendars { get; set; }
}