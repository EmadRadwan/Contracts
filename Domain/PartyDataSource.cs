namespace Domain;

public class PartyDataSource
{
    public string PartyId { get; set; } = null!;
    public string DataSourceId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public string? VisitId { get; set; }
    public string? Comments { get; set; }
    public string? IsCreate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataSource DataSource { get; set; } = null!;
    public Party Party { get; set; } = null!;
}