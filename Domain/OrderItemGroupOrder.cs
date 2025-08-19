namespace Domain;

public class OrderItemGroupOrder
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string GroupOrderId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductGroupOrder GroupOrder { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
}