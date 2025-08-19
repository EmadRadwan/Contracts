namespace Domain;

public class ResponsibilityType
{
    public ResponsibilityType()
    {
        EmplPositionResponsibilities = new HashSet<EmplPositionResponsibility>();
        InverseParentType = new HashSet<ResponsibilityType>();
        ValidResponsibilities = new HashSet<ValidResponsibility>();
    }

    public string ResponsibilityTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ResponsibilityType? ParentType { get; set; }
    public ICollection<EmplPositionResponsibility> EmplPositionResponsibilities { get; set; }
    public ICollection<ResponsibilityType> InverseParentType { get; set; }
    public ICollection<ValidResponsibility> ValidResponsibilities { get; set; }
}