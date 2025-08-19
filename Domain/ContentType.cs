using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ContentType
{
    public ContentType()
    {
        ContentTypeAttrs = new HashSet<ContentTypeAttr>();
        Contents = new HashSet<Content>();
        InverseParentType = new HashSet<ContentType>();
    }

    public string ContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContentType? ParentType { get; set; }
    public ICollection<ContentTypeAttr> ContentTypeAttrs { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<ContentType> InverseParentType { get; set; }
}