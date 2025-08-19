namespace Domain;

public class ContentPurpose
{
    public string ContentId { get; set; } = null!;
    public string ContentPurposeTypeId { get; set; } = null!;
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public ContentPurposeType ContentPurposeType { get; set; } = null!;
}