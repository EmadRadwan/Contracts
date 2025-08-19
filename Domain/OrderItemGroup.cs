namespace Domain;

public class OrderItemGroup
{
    public OrderItemGroup()
    {
        InverseOrderItemGroupNavigation = new HashSet<OrderItemGroup>();
        OrderItems = new HashSet<OrderItem>();
    }

    public string OrderId { get; set; } = null!;
    public string OrderItemGroupSeqId { get; set; } = null!;
    public string? ParentGroupSeqId { get; set; }
    public string? GroupName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public OrderItemGroup? OrderItemGroupNavigation { get; set; }
    public ICollection<OrderItemGroup> InverseOrderItemGroupNavigation { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}