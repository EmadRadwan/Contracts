using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GeoType
{
    public GeoType()
    {
        Geos = new HashSet<Geo>();
        InverseParentType = new HashSet<GeoType>();
    }

    public string GeoTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GeoType? ParentType { get; set; }
    public ICollection<Geo> Geos { get; set; }
    public ICollection<GeoType> InverseParentType { get; set; }
}