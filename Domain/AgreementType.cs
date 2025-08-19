using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class AgreementType
{
    public AgreementType()
    {
        AgreementTypeAttrs = new HashSet<AgreementTypeAttr>();
        Agreements = new HashSet<Agreement>();
        InverseParentType = new HashSet<AgreementType>();
    }

    public string AgreementTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementType? ParentType { get; set; }
    public ICollection<AgreementTypeAttr> AgreementTypeAttrs { get; set; }
    public ICollection<Agreement> Agreements { get; set; }
    public ICollection<AgreementType> InverseParentType { get; set; }
}