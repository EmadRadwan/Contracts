namespace Domain;

public class ElectronicText
{
    public string DataResourceId { get; set; } = null!;
    public string? TextData { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResource DataResource { get; set; } = null!;
}