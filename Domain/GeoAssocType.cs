namespace Domain;

public class GeoAssocType
{
    public GeoAssocType()
    {
        CountryAddressFormats = new HashSet<CountryAddressFormat>();
        GeoAssocs = new HashSet<GeoAssoc>();
    }

    public string GeoAssocTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<CountryAddressFormat> CountryAddressFormats { get; set; }
    public ICollection<GeoAssoc> GeoAssocs { get; set; }
}