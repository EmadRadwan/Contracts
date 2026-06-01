using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlSubAccountCourseLabel
{
    public string GlSubAccountCourseLabelId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}