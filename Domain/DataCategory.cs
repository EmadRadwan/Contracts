namespace Domain;

public class DataCategory
{
    public DataCategory()
    {
        DataResources = new HashSet<DataResource>();
        InverseParentCategory = new HashSet<DataCategory>();
    }

    public string DataCategoryId { get; set; } = null!;
    public string? ParentCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataCategory? ParentCategory { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<DataCategory> InverseParentCategory { get; set; }
}