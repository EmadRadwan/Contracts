namespace Domain;

public class TrainingClassType
{
    public TrainingClassType()
    {
        InverseParentType = new HashSet<TrainingClassType>();
        PersonTrainings = new HashSet<PersonTraining>();
    }

    public string TrainingClassTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TrainingClassType? ParentType { get; set; }
    public ICollection<TrainingClassType> InverseParentType { get; set; }
    public ICollection<PersonTraining> PersonTrainings { get; set; }
}