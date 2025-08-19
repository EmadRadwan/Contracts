namespace Domain;

public class ContentOperation
{
    public ContentOperation()
    {
        ContentPurposeOperations = new HashSet<ContentPurposeOperation>();
    }

    public string ContentOperationId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContentPurposeOperation> ContentPurposeOperations { get; set; }
}