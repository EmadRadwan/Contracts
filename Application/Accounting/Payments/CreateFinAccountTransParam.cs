namespace Application.Accounting.Payments;


public class CreateFinAccountTransParam
{
    // Optional Fields
    public string? FinAccountTransId { get; set; }
    public string? FinAccountTransTypeId { get; set; }
    public string? FinAccountId { get; set; }
    public string? PaymentId { get; set; }
    public string? StatusId { get; set; }
    public string? PartyId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Comments { get; set; }
}