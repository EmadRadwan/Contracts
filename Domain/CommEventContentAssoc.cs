namespace Domain;

public class CommEventContentAssoc
{
    public string ContentId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public string? CommContentAssocTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommContentAssocType? CommContentAssocType { get; set; }
    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public Content Content { get; set; } = null!;
}