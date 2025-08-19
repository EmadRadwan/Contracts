namespace Domain;

public class CharacterSet
{
    public CharacterSet()
    {
        Contents = new HashSet<Content>();
        DataResources = new HashSet<DataResource>();
    }

    public string CharacterSetId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<Content> Contents { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
}