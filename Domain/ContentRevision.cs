namespace Domain;

public class ContentRevision
{
    public ContentRevision()
    {
        ContentRevisionItems = new HashSet<ContentRevisionItem>();
    }

    public string ContentId { get; set; } = null!;
    public string ContentRevisionSeqId { get; set; } = null!;
    public string? CommittedByPartyId { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? CommittedByParty { get; set; }
    public Content Content { get; set; } = null!;
    public ICollection<ContentRevisionItem> ContentRevisionItems { get; set; }
}