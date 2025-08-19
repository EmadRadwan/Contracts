namespace Domain;

public class InspectionResult
{
    public string ResultId { get; set; } = null!;
    public string InspectionId { get; set; } = null!;
    public string ProductQualityStandardId { get; set; } = null!;
    public decimal? MeasuredValueNumeric { get; set; }
    public string? MeasuredValueCategorical { get; set; }
    public bool? MeasuredValueBoolean { get; set; }
    public string Status { get; set; } = null!; // E.g., Pass, Fail
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation properties
    public Inspection Inspection { get; set; } = null!;

    public ProductQualityStandard ProductQualityStandard { get; set; } = null!;

    // One InspectionResult can have many Defects
    public ICollection<Defect> Defects { get; set; } = new List<Defect>();
}