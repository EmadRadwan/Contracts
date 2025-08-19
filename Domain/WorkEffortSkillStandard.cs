namespace Domain;

public class WorkEffortSkillStandard
{
    public string WorkEffortId { get; set; } = null!;
    public string SkillTypeId { get; set; } = null!;
    public double? EstimatedNumPeople { get; set; }
    public double? EstimatedDuration { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SkillType SkillType { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}