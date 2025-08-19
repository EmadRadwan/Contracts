namespace Domain;

public class EmplPositionClassType
{
    public EmplPositionClassType()
    {
        EmplPositionTypeClasses = new HashSet<EmplPositionTypeClass>();
        InverseParentType = new HashSet<EmplPositionClassType>();
    }

    public string EmplPositionClassTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionClassType? ParentType { get; set; }
    public ICollection<EmplPositionTypeClass> EmplPositionTypeClasses { get; set; }
    public ICollection<EmplPositionClassType> InverseParentType { get; set; }
}