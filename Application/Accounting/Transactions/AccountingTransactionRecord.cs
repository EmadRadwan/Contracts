using System.ComponentModel.DataAnnotations;

namespace API.Controllers.Accounting.Transactions;

public class AccountingTransactionRecord
{
    [Key] public string AcctgTransId { get; set; } = null!;

    public string? AcctgTransTypeId { get; set; }
    public string? AcctgTransTypeDescription { get; set; }
    public string? Description { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? ScheduledPostingDate { get; set; }
    public string? GlJournalId { get; set; }
    public string? GlFiscalTypeId { get; set; }
    public string? VoucherRef { get; set; }
    public DateTime? VoucherDate { get; set; }
    public string? GroupStatusId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? PhysicalInventoryId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? ShipmentId { get; set; }
    public string? ReceiptId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? TheirAcctgTransId { get; set; }
}