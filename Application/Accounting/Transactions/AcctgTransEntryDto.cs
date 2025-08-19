namespace API.Controllers.Accounting.Transactions;

public class AcctgTransEntryDto
{
    public string AcctgTransId { get; set; } = null!;
    public string AcctgTransEntrySeqId { get; set; } = null!;
    public string? AcctgTransEntryTypeId { get; set; }
    public string? AcctgTransTypeDescription { get; set; }
    public string? GlFiscalTypeId { get; set; }

    public string? Description { get; set; }
    public string? VoucherRef { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? TheirPartyId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? TheirProductId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountTypeDescription { get; set; }
    public string? GlAccountClassDescription { get; set; }
    public string? GlAccountId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyUomId { get; set; }
    public decimal? OrigAmount { get; set; }
    public string? OrigCurrencyUomId { get; set; }
    public string? DebitCreditFlag { get; set; }
    public DateTime? DueDate { get; set; }
    public string? GroupId { get; set; }
    public string? TaxId { get; set; }
    public string? ReconcileStatusId { get; set; }
    public string? SettlementTermId { get; set; }
    public string? IsSummary { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
}