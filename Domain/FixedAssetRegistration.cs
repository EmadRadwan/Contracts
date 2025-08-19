namespace Domain;

public class FixedAssetRegistration
{
    public string FixedAssetId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? GovAgencyPartyId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public Party? GovAgencyParty { get; set; }
}