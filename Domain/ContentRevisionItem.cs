namespace Domain;

public class ContentRevisionItem
{
    public string ContentId { get; set; } = null!;
    public string ContentRevisionSeqId { get; set; } = null!;
    public string ItemContentId { get; set; } = null!;
    public string? OldDataResourceId { get; set; }
    public string? NewDataResourceId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContentRevision Content { get; set; } = null!;
    public DataResource? NewDataResource { get; set; }
    public DataResource? OldDataResource { get; set; }
}