namespace Domain;

public class JavaResource
{
    public string ResourceName { get; set; } = null!;
    public byte[]? ResourceValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}