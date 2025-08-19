namespace Application.Accounting.Services.Models;

public class PartyFinancialHistoryDetails
{
    public string PartyId { get; set; }
    public string PreferredCurrencyUomId { get; set; }
    public List<InvoiceApplPaymentDto> InvoicesApplPayments { get; set; }
    public List<UnappliedInvoiceDto> UnappliedInvoices { get; set; }
    public List<UnappliedPaymentDto> UnappliedPayments { get; set; }
    public List<BillingAccountDto> BillingAccounts { get; set; }
    public List<ReturnDto> Returns { get; set; }
    public FinancialSummaryDto FinancialSummary { get; set; }
}