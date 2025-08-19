using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccountCategory
{
    public GlAccountCategory()
    {
        GlAccountCategoryMembers = new HashSet<GlAccountCategoryMember>();
    }

    public string GlAccountCategoryId { get; set; } = null!;
    public string? GlAccountCategoryTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountCategoryType? GlAccountCategoryType { get; set; }
    public ICollection<GlAccountCategoryMember> GlAccountCategoryMembers { get; set; }
}