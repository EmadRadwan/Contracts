using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ReturnItemType
{
    public ReturnItemType()
    {
        InverseParentType = new HashSet<ReturnItemType>();
        ReturnItems = new HashSet<ReturnItem>();
    }

    public string ReturnItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ReturnItemType? ParentType { get; set; }
    public ICollection<ReturnItemType> InverseParentType { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
}