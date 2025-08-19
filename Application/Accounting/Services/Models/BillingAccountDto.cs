namespace Application.Accounting.Services.Models;

public class BillingAccountDto
{
    public string BillingAccountId { get; set; }
    public decimal? AccountLimit { get; set; }
    public decimal? AccountBalance { get; set; }
    public string Description { get; set; }
}