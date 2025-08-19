using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.Transactions;

public class AccountingTransactionEntryRecord
{
    [Key] public string AcctgTransId { get; set; } = null!;

    [Key] public string AcctgTransEntrySeqId { get; set; } = null!;

    public string? GlAccountTypeDescription { get; set; }
    public string? AcctgTransactionTypeDescription { get; set; }
    public string? Description { get; set; }
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }
    public string? ProductId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? ShipmentId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountId { get; set; }
    public decimal? Amount { get; set; }
    public string? DebitCreditFlag { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? GlFiscalTypeId { get; set; }
}