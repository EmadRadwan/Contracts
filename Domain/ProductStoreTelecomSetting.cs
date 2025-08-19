namespace Domain;

public class ProductStoreTelecomSetting
{
    public string ProductStoreId { get; set; } = null!;
    public string TelecomMethodTypeId { get; set; } = null!;
    public string TelecomMsgTypeEnumId { get; set; } = null!;
    public string? TelecomCustomMethodId { get; set; }
    public string? TelecomGatewayConfigId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductStore ProductStore { get; set; } = null!;
    public CustomMethod? TelecomCustomMethod { get; set; }
    public TelecomGatewayConfig? TelecomGatewayConfig { get; set; }
    public TelecomMethodType TelecomMethodType { get; set; } = null!;
    public Enumeration TelecomMsgTypeEnum { get; set; } = null!;
}