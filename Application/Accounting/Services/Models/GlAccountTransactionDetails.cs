namespace Application.Accounting.Services.Models;

public class GlAccountTransactionDetails
{
    public decimal OpeningBalance { get; set; }
    public decimal PostedDebits { get; set; }
    public decimal PostedCredits { get; set; }
    public decimal EndingBalance { get; set; }
    public string GlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string GlAccountClassId { get; set; }

    // REFACTOR (2026-08-14): added so the frontend knows which side of the ledger this account
    // lives on (assets/expenses = debit-natured, liabilities/equity/revenue = credit-natured).
    // The date-range Excel export was always assuming debit-natured — correct for bank accounts
    // like 110100, but it would silently run the balance math backwards for a credit account.
    // Backend already computes this (see IsDebitAccount below); this just exposes it.
    public bool IsDebit { get; set; }
    public List<TransactionEntryDto> Transactions { get; set; }
}
