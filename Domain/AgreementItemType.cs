using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class AgreementItemType
{
    public AgreementItemType()
    {
        AgreementItemTypeAttrs = new HashSet<AgreementItemTypeAttr>();
        AgreementItems = new HashSet<AgreementItem>();
        InverseParentType = new HashSet<AgreementItemType>();
    }

    public string AgreementItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AgreementItemType? ParentType { get; set; }
    public ICollection<AgreementItemTypeAttr> AgreementItemTypeAttrs { get; set; }
    public ICollection<AgreementItem> AgreementItems { get; set; }
    public ICollection<AgreementItemType> InverseParentType { get; set; }
}