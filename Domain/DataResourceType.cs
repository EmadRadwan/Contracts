using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DataResourceType
{
    public DataResourceType()
    {
        DataResourceTypeAttrs = new HashSet<DataResourceTypeAttr>();
        DataResources = new HashSet<DataResource>();
        InverseParentType = new HashSet<DataResourceType>();
    }

    public string DataResourceTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DataResourceType? ParentType { get; set; }
    public ICollection<DataResourceTypeAttr> DataResourceTypeAttrs { get; set; }
    public ICollection<DataResource> DataResources { get; set; }
    public ICollection<DataResourceType> InverseParentType { get; set; }
}