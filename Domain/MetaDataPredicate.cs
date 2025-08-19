namespace Domain;

public class MetaDataPredicate
{
    public MetaDataPredicate()
    {
        ContentMetaData = new HashSet<ContentMetaDatum>();
        DataResourceMetaData = new HashSet<DataResourceMetaDatum>();
    }

    public string MetaDataPredicateId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContentMetaDatum> ContentMetaData { get; set; }
    public ICollection<DataResourceMetaDatum> DataResourceMetaData { get; set; }
}