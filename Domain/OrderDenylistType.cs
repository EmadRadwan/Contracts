namespace Domain;

public class OrderDenylistType
{
    public OrderDenylistType()
    {
        OrderDenylists = new HashSet<OrderDenylist>();
    }

    public string OrderDenylistTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<OrderDenylist> OrderDenylists { get; set; }
}