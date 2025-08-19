namespace Domain;

public class EmplPositionTypeClass
{
    public string EmplPositionTypeId { get; set; } = null!;
    public string EmplPositionClassTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public double? StandardHoursPerWeek { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionClassType EmplPositionClassType { get; set; } = null!;
    public EmplPositionType EmplPositionType { get; set; } = null!;
}