using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class TechDataCalendarExcWeek
{
    public string CalendarId { get; set; } = null!;
    public DateTime ExceptionDateStart { get; set; }
    public string? CalendarWeekId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TechDataCalendar Calendar { get; set; } = null!;
    public TechDataCalendarWeek? CalendarWeek { get; set; }
}