namespace Domain;

public class EmploymentAppSourceType
{
    public EmploymentAppSourceType()
    {
        InverseParentType = new HashSet<EmploymentAppSourceType>();
    }

    public string EmploymentAppSourceTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmploymentAppSourceType? ParentType { get; set; }
    public ICollection<EmploymentAppSourceType> InverseParentType { get; set; }
}