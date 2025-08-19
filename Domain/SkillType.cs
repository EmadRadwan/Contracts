namespace Domain;

public class SkillType
{
    public SkillType()
    {
        InverseParentType = new HashSet<SkillType>();
        JobRequisitions = new HashSet<JobRequisition>();
        PartySkills = new HashSet<PartySkill>();
        QuoteItems = new HashSet<QuoteItem>();
        WorkEffortSkillStandards = new HashSet<WorkEffortSkillStandard>();
    }

    public string SkillTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SkillType? ParentType { get; set; }
    public ICollection<SkillType> InverseParentType { get; set; }
    public ICollection<JobRequisition> JobRequisitions { get; set; }
    public ICollection<PartySkill> PartySkills { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<WorkEffortSkillStandard> WorkEffortSkillStandards { get; set; }
}