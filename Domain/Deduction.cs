namespace Domain;

public class Deduction
{
    public string DeductionId { get; set; } = null!;
    public string? DeductionTypeId { get; set; }
    public string? PaymentId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DeductionType? DeductionType { get; set; }
    public Payment? Payment { get; set; }
}