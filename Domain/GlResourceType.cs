using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlResourceType
{
    public GlResourceType()
    {
        GlAccounts = new HashSet<GlAccount>();
    }

    public string GlResourceTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<GlAccount> GlAccounts { get; set; }
}