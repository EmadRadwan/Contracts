using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccountCourseLabel
{
    public string GlAccountCourseLabelId { get; set; } = null!; // e.g., "CASH_EQ", "RECEIVABLES"
    public string? Description { get; set; }                   // e.g., "Cash & Cash Equivalents"
    public string? DescriptionArabic { get; set; }        // e.g., "Balance Sheet", "Profit and Loss"
    public int SignMultiplier { get; set; } = 1;               // 1 for Debit, -1 for Credit
    public string? SortOrder { get; set; } 
    
    public ICollection<GlAccount> GlAccounts { get; set; } = new HashSet<GlAccount>();
}