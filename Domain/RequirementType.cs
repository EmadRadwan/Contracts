namespace Domain;

public class RequirementType
{
    public RequirementType()
    {
        InverseParentType = new HashSet<RequirementType>();
        RequirementTypeAttrs = new HashSet<RequirementTypeAttr>();
        Requirements = new HashSet<Requirement>();
    }

    public string RequirementTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public RequirementType? ParentType { get; set; }
    public ICollection<RequirementType> InverseParentType { get; set; }
    public ICollection<RequirementTypeAttr> RequirementTypeAttrs { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
}