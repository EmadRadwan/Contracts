namespace Application.Accounting.Services.Models;

/// <summary>
/// DTO that corresponds to the Ofbiz approach of quickCreateAcctgTransAndEntries.
/// There's no array of entries, just single fields for debit/credit accounts and amount.
/// </summary>
public class CreateQuickAcctgTransAndEntriesParams
{
    // For the AcctgTrans:
    public string GlFiscalTypeId { get; set; } = "ACTUAL"; 
    public string AcctgTransTypeId { get; set; }     // e.g. RECEIPT, PAYMENT_ACCTG_TRANS, etc.
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? ShipmentId { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? AcctgTransEntryTypeId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public string? IsPosted { get; set; } = "N";

    // For the two offsetting AcctgTransEntry records:
    public decimal Amount { get; set; }              // The total amount for both debit & credit
    public string DebitGlAccountId { get; set; }     // The GL account for the debit entry
    public string CreditGlAccountId { get; set; }    // The GL account for the credit entry
}