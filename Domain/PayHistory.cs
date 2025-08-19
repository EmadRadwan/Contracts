namespace Domain;

public class PayHistory
{
    public string RoleTypeIdFrom { get; set; } = null!;
    public string RoleTypeIdTo { get; set; } = null!;
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public DateTime EmplFromDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? SalaryStepSeqId { get; set; }
    public string? PayGradeId { get; set; }
    public string? PeriodTypeId { get; set; }
    public decimal? Amount { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Employment Employment { get; set; } = null!;
    public PayGrade? PayGrade { get; set; }
    public PeriodType? PeriodType { get; set; }
}