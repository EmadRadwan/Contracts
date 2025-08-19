namespace Domain;

public class DataResourceMetaDatum
{
    public string DataResourceId { get; set; } = null!;
    public string MetaDataPredicateId { get; set; } = null!;
    public string? MetaDataValue { get; set; }
    public string? DataSourceId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResource DataResource { get; set; } = null!;
    public DataSource? DataSource { get; set; }
    public MetaDataPredicate MetaDataPredicate { get; set; } = null!;
}