namespace Domain;

public class CommunicationEventOrder
{
    public string OrderId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public OrderHeader Order { get; set; } = null!;
}