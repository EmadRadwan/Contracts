namespace Domain;

public class DataTemplateType
{
    public DataTemplateType()
    {
        DataResources = new HashSet<DataResource>();
    }

    public string DataTemplateTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? Extension { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<DataResource> DataResources { get; set; }
}