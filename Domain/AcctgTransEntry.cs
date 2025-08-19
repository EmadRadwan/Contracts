using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class AcctgTransEntry
{
    public AcctgTransEntry()
    {
        GlReconciliationEntries = new HashSet<GlReconciliationEntry>();
    }

    public string? AcctgTransId { get; set; } = null!;
    public string? AcctgTransEntrySeqId { get; set; } = null!;
    public string? AcctgTransEntryTypeId { get; set; }
    public string? Description { get; set; }
    public string? VoucherRef { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? TheirPartyId { get; set; }
    public string? ProductId { get; set; }
    public string? TheirProductId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? GlAccountTypeId { get; set; }
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
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AcctgTran? AcctgTrans { get; set; } = null!;
    public AcctgTransEntryType? AcctgTransEntryType { get; set; }
    public Uom? CurrencyUom { get; set; }
    public GlAccount? GlAccount { get; set; }
    public GlAccountOrganization? GlAccountOrganization { get; set; }
    public GlAccountType? GlAccountType { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public Uom? OrigCurrencyUom { get; set; }
    public Party? Party { get; set; }
    public StatusItem? ReconcileStatus { get; set; }
    public RoleType? RoleType { get; set; }
    public SettlementTerm? SettlementTerm { get; set; }
    public ICollection<GlReconciliationEntry> GlReconciliationEntries { get; set; }
}