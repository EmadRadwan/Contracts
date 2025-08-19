namespace Domain;

public class SalaryStepNew
{
    public string SalaryStepSeqId { get; set; } = null!;
    public string PayGradeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? DateModified { get; set; }
    public decimal? Amount { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PayGrade PayGrade { get; set; } = null!;
}