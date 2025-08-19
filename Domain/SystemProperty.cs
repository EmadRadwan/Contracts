namespace Domain;

public class SystemProperty
{
    public string SystemResourceId { get; set; } = null!;
    public string SystemPropertyId { get; set; } = null!;
    public string? SystemPropertyValue { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}