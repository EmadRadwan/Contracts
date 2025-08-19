namespace Domain;

public class OrderItemAssoc
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string ShipGroupSeqId { get; set; } = null!;
    public string ToOrderId { get; set; } = null!;
    public string ToOrderItemSeqId { get; set; } = null!;
    public string ToShipGroupSeqId { get; set; } = null!;
    public string OrderItemAssocTypeId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public OrderItemAssocType OrderItemAssocType { get; set; } = null!;
    public OrderHeader ToOrder { get; set; } = null!;
}