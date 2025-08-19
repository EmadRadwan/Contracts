namespace Domain;

public class EmplPositionReportingStruct
{
    public string EmplPositionIdReportingTo { get; set; } = null!;
    public string EmplPositionIdManagedBy { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public string? PrimaryFlag { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPosition EmplPositionIdManagedByNavigation { get; set; } = null!;
    public EmplPosition EmplPositionIdReportingToNavigation { get; set; } = null!;
}