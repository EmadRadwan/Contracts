using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class TechDataCalendar
{
    public TechDataCalendar()
    {
        FixedAssets = new HashSet<FixedAsset>();
        TechDataCalendarExcDays = new HashSet<TechDataCalendarExcDay>();
        TechDataCalendarExcWeeks = new HashSet<TechDataCalendarExcWeek>();
    }

    public string CalendarId { get; set; } = null!;
    public string? Description { get; set; }
    public string? CalendarWeekId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TechDataCalendarWeek? CalendarWeek { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<TechDataCalendarExcDay> TechDataCalendarExcDays { get; set; }
    public ICollection<TechDataCalendarExcWeek> TechDataCalendarExcWeeks { get; set; }
}