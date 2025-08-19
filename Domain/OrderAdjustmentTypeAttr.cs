namespace Domain;

public class OrderAdjustmentTypeAttr
{
    public string OrderAdjustmentTypeId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderAdjustmentType OrderAdjustmentType { get; set; } = null!;
}