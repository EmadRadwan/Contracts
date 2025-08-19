namespace Domain;

public class OrderHeaderNote
{
    public string OrderId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public string? InternalNote { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public NoteDatum Note { get; set; } = null!;
    public OrderHeader Order { get; set; } = null!;
}