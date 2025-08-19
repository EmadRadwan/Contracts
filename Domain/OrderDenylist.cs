namespace Domain;

public class OrderDenylist
{
    public string DenylistString { get; set; } = null!;
    public string OrderDenylistTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderDenylistType OrderDenylistType { get; set; } = null!;
}