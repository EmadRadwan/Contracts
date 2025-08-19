namespace Domain;

public class PersonTraining
{
    public string PartyId { get; set; } = null!;
    public string? TrainingRequestId { get; set; }
    public string TrainingClassTypeId { get; set; } = null!;
    public string? WorkEffortId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? ApproverId { get; set; }
    public string? ApprovalStatus { get; set; }
    public string? Reason { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Person? Approver { get; set; }
    public Party Party { get; set; } = null!;
    public TrainingClassType TrainingClassType { get; set; } = null!;
    public TrainingRequest? TrainingRequest { get; set; }
    public WorkEffort? WorkEffort { get; set; }
}