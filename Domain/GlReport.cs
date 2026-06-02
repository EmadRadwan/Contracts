using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlReport
{
    public string GlReportId { get; set; } = null!; // e.g., "BS", "PL"
    public string? Description { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    public string? DescriptionArabic { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    public string? SortOrder { get; set; } 
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}