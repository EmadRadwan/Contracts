namespace Domain;

public class TaxAuthorityAssoc
{
    public string TaxAuthGeoId { get; set; } = null!;
    public string TaxAuthPartyId { get; set; } = null!;
    public string ToTaxAuthGeoId { get; set; } = null!;
    public string ToTaxAuthPartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? TaxAuthorityAssocTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TaxAuthority TaxAuth { get; set; } = null!;
    public TaxAuthorityAssocType? TaxAuthorityAssocType { get; set; }
    public TaxAuthority ToTaxAuth { get; set; } = null!;
}