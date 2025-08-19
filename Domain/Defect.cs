namespace Domain;

public class Defect
{
    public string DefectId { get; set; } = null!;
    public string ResultId { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Severity { get; set; } = null!; // E.g., Minor, Major, Critical
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation properties
    public InspectionResult InspectionResult { get; set; } = null!;

    // One Defect can have many CorrectiveActions
    public ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
}