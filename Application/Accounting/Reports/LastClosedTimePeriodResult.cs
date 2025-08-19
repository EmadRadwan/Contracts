using Domain;

namespace Application.Shipments.Reports;

public class LastClosedTimePeriodResult
{
    public DateTime? LastClosedDate { get; set; }
    public CustomTimePeriod LastClosedTimePeriod { get; set; }
}