namespace Domain;

public class ImageDataResource
{
    public string DataResourceId { get; set; } = null!;
    public byte[]? ImageData { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResource DataResource { get; set; } = null!;
}