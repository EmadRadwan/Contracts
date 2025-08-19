namespace Domain;

public class CommunicationEventReturn
{
    public string ReturnId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public ReturnHeader Return { get; set; } = null!;
}