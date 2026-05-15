using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlSubClass2
{
    public string GlSubClass2Id { get; set; } = null!; // e.g., "MKT", "ADMIN"
    public string? Description { get; set; }          // e.g., "Marketing", "Administration"
    public string? DescriptionArabic { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}