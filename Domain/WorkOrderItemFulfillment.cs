namespace Domain;

public class WorkOrderItemFulfillment
{
    public string WorkEffortId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string? ShipGroupSeqId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}