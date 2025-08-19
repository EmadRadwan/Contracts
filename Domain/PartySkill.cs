namespace Domain;

public class PartySkill
{
    public string PartyId { get; set; } = null!;
    public string SkillTypeId { get; set; } = null!;
    public int? YearsExperience { get; set; }
    public int? Rating { get; set; }
    public int? SkillLevel { get; set; }
    public DateTime? StartedUsingDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public SkillType SkillType { get; set; } = null!;
}