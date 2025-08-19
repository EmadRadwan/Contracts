namespace Domain;

public class EmplPositionTypeRateNew
{
    public string EmplPositionTypeId { get; set; } = null!;
    public string RateTypeId { get; set; } = null!;
    public string? PayGradeId { get; set; }
    public string? SalaryStepSeqId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public EmplPositionType EmplPositionType { get; set; } = null!;
}