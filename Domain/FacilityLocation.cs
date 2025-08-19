using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class FacilityLocation
{
    public FacilityLocation()
    {
        FacilityLocationGeoPoints = new HashSet<FacilityLocationGeoPoint>();
        ProductFacilityLocations = new HashSet<ProductFacilityLocation>();
    }

    public string FacilityId { get; set; } = null!;
    public string LocationSeqId { get; set; } = null!;
    public string? LocationTypeEnumId { get; set; }
    public string? AreaId { get; set; }
    public string? AisleId { get; set; }
    public string? SectionId { get; set; }
    public string? LevelId { get; set; }
    public string? PositionId { get; set; }
    public string? GeoPointId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public GeoPoint? GeoPoint { get; set; }
    public Enumeration? LocationTypeEnum { get; set; }
    public ICollection<FacilityLocationGeoPoint> FacilityLocationGeoPoints { get; set; }
    public ICollection<ProductFacilityLocation> ProductFacilityLocations { get; set; }
}