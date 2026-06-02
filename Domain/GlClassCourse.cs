using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlClassCourse
{
    public string GlClassCourseId { get; set; } = null!; // e.g., "ASSET", "OP_ACCT"
    public string? Description { get; set; }           // e.g., "Assets", "Operating account"
    public string? DescriptionArabic { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    public string? SortOrder { get; set; } 
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}