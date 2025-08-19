namespace Domain;

public class FixedAssetTypeGlAccount
{
    public string FixedAssetTypeId { get; set; } = null!;
    public string FixedAssetId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? AssetGlAccountId { get; set; }
    public string? AccDepGlAccountId { get; set; }
    public string? DepGlAccountId { get; set; }
    public string? ProfitGlAccountId { get; set; }
    public string? LossGlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? AccDepGlAccount { get; set; }
    public GlAccount? AssetGlAccount { get; set; }
    public GlAccount? DepGlAccount { get; set; }
    public GlAccount? LossGlAccount { get; set; }
    public Party OrganizationParty { get; set; } = null!;
    public GlAccount? ProfitGlAccount { get; set; }
}