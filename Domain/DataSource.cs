namespace Domain;

public class DataSource
{
    public DataSource()
    {
        ContentAssocs = new HashSet<ContentAssoc>();
        ContentMetaData = new HashSet<ContentMetaDatum>();
        Contents = new HashSet<Content>();
        DataResourceMetaData = new HashSet<DataResourceMetaDatum>();
        DataResources = new HashSet<DataResource>();
        GeoPoints = new HashSet<GeoPoint>();
        Parties = new HashSet<Party>();
        PartyDataSources = new HashSet<PartyDataSource>();
        SalesOpportunities = new HashSet<SalesOpportunity>();
    }

    public string DataSourceId { get; set; } = null!;
    public string? DataSourceTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataSourceType? DataSourceType { get; set; }
    public ICollection<ContentAssoc> ContentAssocs { get; set; }
    public ICollection<ContentMetaDatum> ContentMetaData { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<DataResourceMetaDatum> DataResourceMetaData { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<GeoPoint> GeoPoints { get; set; }
    public ICollection<Party> Parties { get; set; }
    public ICollection<PartyDataSource> PartyDataSources { get; set; }
    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
}