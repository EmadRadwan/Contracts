namespace Domain;

public class OrderItemAssocType
{
    public OrderItemAssocType()
    {
        InverseParentType = new HashSet<OrderItemAssocType>();
        OrderItemAssocs = new HashSet<OrderItemAssoc>();
    }

    public string OrderItemAssocTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderItemAssocType? ParentType { get; set; }
    public ICollection<OrderItemAssocType> InverseParentType { get; set; }
    public ICollection<OrderItemAssoc> OrderItemAssocs { get; set; }
}