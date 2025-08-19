namespace Domain;

public class CustRequestCommEvent
{
    public string CustRequestId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public CustRequest CustRequest { get; set; } = null!;
}