namespace Domain;

public class ProductPriceAction
{
    public ProductPriceAction()
    {
        OrderItemPriceInfos = new HashSet<OrderItemPriceInfo>();
    }

    public string ProductPriceRuleId { get; set; } = null!;
    public string ProductPriceActionSeqId { get; set; } = null!;
    public string? ProductPriceActionTypeId { get; set; }
    public decimal? Amount { get; set; }
    public string? RateCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductPriceActionType? ProductPriceActionType { get; set; }
    public ProductPriceRule ProductPriceRule { get; set; } = null!;
    public ICollection<OrderItemPriceInfo> OrderItemPriceInfos { get; set; }
}