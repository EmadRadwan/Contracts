using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class CustomMethodType
{
    public CustomMethodType()
    {
        CustomMethods = new HashSet<CustomMethod>();
        InverseParentType = new HashSet<CustomMethodType>();
    }

    public string CustomMethodTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethodType? ParentType { get; set; }
    public ICollection<CustomMethod> CustomMethods { get; set; }
    public ICollection<CustomMethodType> InverseParentType { get; set; }
}