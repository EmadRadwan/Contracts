using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlXbrlClass
{
    public GlXbrlClass()
    {
        GlAccounts = new HashSet<GlAccount>();
        InverseParentGlXbrlClass = new HashSet<GlXbrlClass>();
    }

    public string GlXbrlClassId { get; set; } = null!;
    public string? ParentGlXbrlClassId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlXbrlClass? ParentGlXbrlClass { get; set; }
    public ICollection<GlAccount> GlAccounts { get; set; }
    public ICollection<GlXbrlClass> InverseParentGlXbrlClass { get; set; }
}