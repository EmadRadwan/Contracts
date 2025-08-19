namespace Domain;

public class CommunicationEventProduct
{
    public string ProductId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public Product Product { get; set; } = null!;
}