namespace Domain;

public class DataResourcePurpose
{
    public string DataResourceId { get; set; } = null!;
    public string ContentPurposeTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContentPurposeType ContentPurposeType { get; set; } = null!;
    public DataResource DataResource { get; set; } = null!;
}