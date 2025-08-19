namespace Application.Accounting.BillingAccounts;

public class ListBillingAccountInvoicesRequest
{
    public string BillingAccountId { get; set; }
    public string StatusId { get; set; } // Optional
}