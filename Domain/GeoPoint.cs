namespace Domain;

public class GeoPoint
{
    public GeoPoint()
    {
        ContainerGeoPoints = new HashSet<ContainerGeoPoint>();
        Facilities = new HashSet<Facility>();
        FacilityLocationGeoPoints = new HashSet<FacilityLocationGeoPoint>();
        FacilityLocations = new HashSet<FacilityLocation>();
        FixedAssetGeoPoints = new HashSet<FixedAssetGeoPoint>();
        PartyGeoPoints = new HashSet<PartyGeoPoint>();
        PostalAddresses = new HashSet<PostalAddress>();
    }

    public string GeoPointId { get; set; } = null!;
    public string? GeoPointTypeEnumId { get; set; }
    public string? Description { get; set; }
    public string? DataSourceId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Elevation { get; set; }
    public string? ElevationUomId { get; set; }
    public string? Information { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataSource? DataSource { get; set; }
    public Uom? ElevationUom { get; set; }
    public Enumeration? GeoPointTypeEnum { get; set; }
    public ICollection<ContainerGeoPoint> ContainerGeoPoints { get; set; }
    public ICollection<Facility> Facilities { get; set; }
    public ICollection<FacilityLocationGeoPoint> FacilityLocationGeoPoints { get; set; }
    public ICollection<FacilityLocation> FacilityLocations { get; set; }
    public ICollection<FixedAssetGeoPoint> FixedAssetGeoPoints { get; set; }
    public ICollection<PartyGeoPoint> PartyGeoPoints { get; set; }
    public ICollection<PostalAddress> PostalAddresses { get; set; }
}