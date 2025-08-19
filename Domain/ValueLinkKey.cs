namespace Domain;

public class ValueLinkKey
{
    public string MerchantId { get; set; } = null!;
    public string? PublicKey { get; set; }
    public string? PrivateKey { get; set; }
    public string? ExchangeKey { get; set; }
    public string? WorkingKey { get; set; }
    public int? WorkingKeyIndex { get; set; }
    public string? LastWorkingKey { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByTerminal { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByTerminal { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}