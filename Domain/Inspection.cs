namespace Domain;

public class Inspection
{
    public string InspectionId { get; set; } = null!;
    public DateTime InspectionDate { get; set; }
    public string InspectorId { get; set; } = null!;
    public string? WorkEffortId { get; set; } // Optional
    public string? LotId { get; set; } // Optional
    public string Status { get; set; } = null!; // E.g., In Progress, Completed
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation properties
    public Party Inspector { get; set; } = null!;
    public WorkEffort? WorkEffort { get; set; }

    public Lot? Lot { get; set; }

    // One Inspection can have many InspectionResults
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}