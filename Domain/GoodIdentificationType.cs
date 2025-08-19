using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GoodIdentificationType
{
    public GoodIdentificationType()
    {
        GoodIdentifications = new HashSet<GoodIdentification>();
        InverseParentType = new HashSet<GoodIdentificationType>();
    }

    public string GoodIdentificationTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GoodIdentificationType? ParentType { get; set; }
    public ICollection<GoodIdentification> GoodIdentifications { get; set; }
    public ICollection<GoodIdentificationType> InverseParentType { get; set; }
}