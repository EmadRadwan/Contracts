using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccountClass
{
    public GlAccountClass()
    {
        GlAccounts = new HashSet<GlAccount>();
        InverseParentClass = new HashSet<GlAccountClass>();
    }

    public string GlAccountClassId { get; set; } = null!;
    public string? ParentClassId { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? IsAssetClass { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountClass? ParentClass { get; set; }
    public ICollection<GlAccount> GlAccounts { get; set; }
    public ICollection<GlAccountClass> InverseParentClass { get; set; }
}