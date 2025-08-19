namespace Domain;

public class GlReconciliation
{
    public GlReconciliation()
    {
        FinAccountTrans = new HashSet<FinAccountTran>();
        GlReconciliationEntries = new HashSet<GlReconciliationEntry>();
    }

    public string GlReconciliationId { get; set; } = null!;
    public string? GlReconciliationName { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public string? GlAccountId { get; set; }
    public string? StatusId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public decimal? ReconciledBalance { get; set; }
    public decimal? OpeningBalance { get; set; }
    public DateTime? ReconciledDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? GlAccount { get; set; }
    public Party? OrganizationParty { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<GlReconciliationEntry> GlReconciliationEntries { get; set; }
}