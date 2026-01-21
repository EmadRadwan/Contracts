namespace Application.Accounting.Services.Models;

public class PartyFinancialHistoryDetails
{
    public string PartyId { get; set; } = string.Empty;
    public string? MainRole { get; set; } 
    public string? LedgerPerspective { get; set; } 
    public string? PreferredCurrencyUomId { get; set; }

    public List<InvoiceApplPaymentDto> InvoicesApplPayments { get; set; } = new();
    public List<UnappliedInvoiceDto> UnappliedInvoices { get; set; } = new();
    public List<UnappliedPaymentDto> UnappliedPayments { get; set; } = new();
    public List<BillingAccountDto> BillingAccounts { get; set; } = new();
    public List<ReturnDto> Returns { get; set; } = new();

    // Already existing special transaction types
    public List<OpeningBalanceDto> OpeningBalances { get; set; } = new();
    public List<RentalPropertyPostingDto> RentalPropertyPostings { get; set; } = new();
    public List<ApartmentSalePostingDto> ApartmentSalePostings { get; set; } = new();

    // NEW: Partner revenue accruals (credit side typically)
    public List<PartnerAccrualPostingDto> PartnerAccrualPostings { get; set; } = new();
    public List<ChequeIssuedPostingDto> ChequeIssuedPostings { get; set; } = new();

    public FinancialSummaryDto FinancialSummary { get; set; } = new();
}