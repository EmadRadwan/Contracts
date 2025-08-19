namespace Domain;

public class CountryAddressFormat
{
    public string GeoId { get; set; } = null!;
    public string? GeoAssocTypeId { get; set; }
    public string? RequireStateProvinceId { get; set; }
    public string? RequirePostalCode { get; set; }
    public string? PostalCodeRegex { get; set; }
    public string? HasPostalCodeExt { get; set; }
    public string? RequirePostalCodeExt { get; set; }
    public string? AddressFormat { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo Geo { get; set; } = null!;
    public GeoAssocType? GeoAssocType { get; set; }
}