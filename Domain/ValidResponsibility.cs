namespace Domain;

public class ValidResponsibility
{
    public string EmplPositionTypeId { get; set; } = null!;
    public string ResponsibilityTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionType EmplPositionType { get; set; } = null!;
    public ResponsibilityType ResponsibilityType { get; set; } = null!;
}