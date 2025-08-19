namespace Domain;

public class OrderItemPriceInfo
{
    public string OrderItemPriceInfoId { get; set; } = null!;
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string? ProductPriceRuleId { get; set; }
    public string? ProductPriceActionSeqId { get; set; }
    public decimal? ModifyAmount { get; set; }
    public string? Description { get; set; }
    public string? RateCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderItem? OrderI { get; set; }
    public ProductPriceAction? ProductPrice { get; set; }
}