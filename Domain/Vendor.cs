namespace Domain;

public class Vendor
{
    public string PartyId { get; set; } = null!;
    public string? ManifestCompanyName { get; set; }
    public string? ManifestCompanyTitle { get; set; }
    public string? ManifestLogoUrl { get; set; }
    public string? ManifestPolicies { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
}