namespace Domain;

public class ContentPurposeType
{
    public ContentPurposeType()
    {
        ContentPurposeOperations = new HashSet<ContentPurposeOperation>();
        ContentPurposes = new HashSet<ContentPurpose>();
        DataResourcePurposes = new HashSet<DataResourcePurpose>();
    }

    public string ContentPurposeTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ContentPurposeOperation> ContentPurposeOperations { get; set; }
    public ICollection<ContentPurpose> ContentPurposes { get; set; }
    public ICollection<DataResourcePurpose> DataResourcePurposes { get; set; }
}