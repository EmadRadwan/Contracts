namespace Domain;

public class DataSourceType
{
    public DataSourceType()
    {
        DataSources = new HashSet<DataSource>();
    }

    public string DataSourceTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<DataSource> DataSources { get; set; }
}