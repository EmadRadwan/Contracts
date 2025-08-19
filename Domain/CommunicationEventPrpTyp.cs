namespace Domain;

public class CommunicationEventPrpTyp
{
    public CommunicationEventPrpTyp()
    {
        CommunicationEventPurposes = new HashSet<CommunicationEventPurpose>();
        InverseParentType = new HashSet<CommunicationEventPrpTyp>();
    }

    public string CommunicationEventPrpTypId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEventPrpTyp? ParentType { get; set; }
    public ICollection<CommunicationEventPurpose> CommunicationEventPurposes { get; set; }
    public ICollection<CommunicationEventPrpTyp> InverseParentType { get; set; }
}