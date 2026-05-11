using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlSubClass
{
    public string GlSubClassId { get; set; } = null!; // e.g., "CUR_ASSET", "SALES"
    public string? Description { get; set; }         // e.g., "Current Assets", "Sales"
    public string? DescriptionArabic { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}