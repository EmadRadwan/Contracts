namespace Domain;

public class CorrectiveAction
{
    public string ActionId { get; set; } = null!;
    public string DefectId { get; set; } = null!;
    public string ActionDescription { get; set; } = null!;
    public string Status { get; set; } = null!; // E.g., Planned, In Progress, Completed
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation property
    public Defect Defect { get; set; } = null!;
}