namespace Domain;

public class OrderRequirementCommitment
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public string RequirementId { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public OrderItem OrderI { get; set; } = null!;
    public Requirement Requirement { get; set; } = null!;
}