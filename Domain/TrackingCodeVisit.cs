namespace Domain;

public class TrackingCodeVisit
{
    public string TrackingCodeId { get; set; } = null!;
    public string VisitId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public string? SourceEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Enumeration? SourceEnum { get; set; }
    public TrackingCode TrackingCode { get; set; } = null!;
}