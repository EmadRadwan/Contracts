using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class AcctgTran
{
    public AcctgTran()
    {
        AcctgTransAttributes = new HashSet<AcctgTransAttribute>();
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
    }

    public string AcctgTransId { get; set; } = null!;
    public string? AcctgTransTypeId { get; set; }
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
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AcctgTransType? AcctgTransType { get; set; }
    public FinAccountTran? FinAccountTrans { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public GlFiscalType? GlFiscalType { get; set; }
    public GlJournal? GlJournal { get; set; }
    public StatusItem? GroupStatus { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public InventoryItemVariance? InventoryItemVariance { get; set; }
    public Invoice? Invoice { get; set; }
    public Party? Party { get; set; }
    public Payment? Payment { get; set; }
    public PhysicalInventory? PhysicalInventory { get; set; }
    public ShipmentReceipt? Receipt { get; set; }
    public RoleType? RoleType { get; set; }
    public Shipment? Shipment { get; set; }
    public WorkEffort? WorkEffort { get; set; }
    public ICollection<AcctgTransAttribute> AcctgTransAttributes { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
}