namespace Application.Shipments.BillingAccounts;

public class BillingAccountDto
{
    public string BillingAccountId { get; set; } = null!;
    public decimal? AccountLimit { get; set; }
    public string? AccountCurrencyUomId { get; set; }
    public string? AccountCurrencyUomDescription { get; set; }
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Description { get; set; }
}