namespace Domain;

public class ProductCategoryGlAccount
{
    public string ProductCategoryId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string GlAccountTypeId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? GlAccount { get; set; }
    public GlAccountType GlAccountType { get; set; } = null!;
    public Party OrganizationParty { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}