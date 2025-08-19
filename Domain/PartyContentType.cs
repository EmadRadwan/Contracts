namespace Domain;

public class PartyContentType
{
    public PartyContentType()
    {
        InverseParentType = new HashSet<PartyContentType>();
        PartyContents = new HashSet<PartyContent>();
    }

    public string PartyContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyContentType? ParentType { get; set; }
    public ICollection<PartyContentType> InverseParentType { get; set; }
    public ICollection<PartyContent> PartyContents { get; set; }
}