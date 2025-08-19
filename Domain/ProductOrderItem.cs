namespace Domain;

public class ProductOrderItem
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string EngagementId { get; set; } = null!;
    public string EngagementItemSeqId { get; set; } = null!;
    public string? ProductId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Engagement { get; set; } = null!;
    public OrderItem EngagementI { get; set; } = null!;
    public OrderHeader Order { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
    public Product? Product { get; set; }
}