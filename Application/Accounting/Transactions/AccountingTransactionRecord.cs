using System.ComponentModel.DataAnnotations;

namespace Application.Accounting.Transactions;

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
    public string? SalesRequestId { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public decimal? DebitTotal { get; set; }
    public decimal? CreditTotal { get; set; }
    public decimal? NetAmount { get; set; }
    
    public string CertificateNumber { get; set; }
    public string ProjectNumber { get; set; }
    public string ProjectName { get; set; }
    public string? PartyId { get; set; }
    public string PartyName { get; set; }
    public string? CostCenterId { get; set; }
    public string? CostCenterDescription { get; set; }
    public string? RoleTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? FinAccountTransId { get; set; }
    public string? ShipmentId { get; set; }
    public string? ReceiptId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? TheirAcctgTransId { get; set; }
}