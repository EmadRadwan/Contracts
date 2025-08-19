using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ReturnHeaderType
{
    public ReturnHeaderType()
    {
        InverseParentType = new HashSet<ReturnHeaderType>();
        ReturnHeaders = new HashSet<ReturnHeader>();
        ReturnItemTypeMaps = new HashSet<ReturnItemTypeMap>();
    }

    public string ReturnHeaderTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ReturnHeaderType? ParentType { get; set; }
    public ICollection<ReturnHeaderType> InverseParentType { get; set; }
    public ICollection<ReturnHeader> ReturnHeaders { get; set; }
    public ICollection<ReturnItemTypeMap> ReturnItemTypeMaps { get; set; }
}