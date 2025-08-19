namespace Domain;

public class PayGrade
{
    public PayGrade()
    {
        PayHistories = new HashSet<PayHistory>();
        SalaryStepNews = new HashSet<SalaryStepNew>();
    }

    public string PayGradeId { get; set; } = null!;
    public string? PayGradeName { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PayHistory> PayHistories { get; set; }
    public ICollection<SalaryStepNew> SalaryStepNews { get; set; }
}