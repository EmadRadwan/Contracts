namespace Domain;

public class TelecomGatewayConfig
{
    public TelecomGatewayConfig()
    {
        ProductStoreTelecomSettings = new HashSet<ProductStoreTelecomSetting>();
    }

    public string TelecomGatewayConfigId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductStoreTelecomSetting> ProductStoreTelecomSettings { get; set; }
}