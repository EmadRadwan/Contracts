namespace Domain;

public class TrainingRequest
{
    public TrainingRequest()
    {
        PersonTrainings = new HashSet<PersonTraining>();
    }

    public string TrainingRequestId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PersonTraining> PersonTrainings { get; set; }
}