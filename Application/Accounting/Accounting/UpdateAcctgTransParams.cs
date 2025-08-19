namespace Application.Accounting.Accounting;

public class UpdateAcctgTransParams
{
    // PK field (from <auto-attributes include="pk" ...>)
    public string AcctgTransId { get; set; } = null!;

    // Non-PK fields (from <auto-attributes include="nonpk" ...>)
    public string? AcctgTransTypeId { get; set; }
    public string? GlFiscalTypeId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? IsPosted { get; set; }
    public string? GlJournalId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? ShipmentId { get; set; }

    public string? WorkEffortId { get; set; }
    // Add more if your AcctgTrans entity has more columns
}