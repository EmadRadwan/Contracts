namespace Domain;

public class PartyIdentificationType
{
    public PartyIdentificationType()
    {
        InverseParentType = new HashSet<PartyIdentificationType>();
        PartyIdentifications = new HashSet<PartyIdentification>();
    }

    public string PartyIdentificationTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyIdentificationType? ParentType { get; set; }
    public ICollection<PartyIdentificationType> InverseParentType { get; set; }
    public ICollection<PartyIdentification> PartyIdentifications { get; set; }
}