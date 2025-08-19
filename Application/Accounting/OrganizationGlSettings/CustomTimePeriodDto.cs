namespace Application.Shipments.OrganizationGlSettings;

public class CustomTimePeriodDto
{
    public string CustomTimePeriodId { get; set; } = null!;
    public string? ParentPeriodId { get; set; }
    public string? PeriodTypeId { get; set; }
    public string? PeriodTypeDescription { get; set; }
    public int? PeriodNum { get; set; }
    public string? PeriodName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsClosed { get; set; }
}