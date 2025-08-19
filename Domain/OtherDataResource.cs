namespace Domain;

public class OtherDataResource
{
    public string DataResourceId { get; set; } = null!;
    public byte[]? DataResourceContent { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResource DataResource { get; set; } = null!;
}