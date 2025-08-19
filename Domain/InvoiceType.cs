using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InvoiceType
{
    public InvoiceType()
    {
        InverseParentType = new HashSet<InvoiceType>();
        InvoiceItemTypeMaps = new HashSet<InvoiceItemTypeMap>();
        InvoiceTypeAttrs = new HashSet<InvoiceTypeAttr>();
        Invoices = new HashSet<Invoice>();
        PartyPrefDocTypeTpls = new HashSet<PartyPrefDocTypeTpl>();
    }

    public string InvoiceTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceType? ParentType { get; set; }
    public ICollection<InvoiceType> InverseParentType { get; set; }
    public ICollection<InvoiceItemTypeMap> InvoiceItemTypeMaps { get; set; }
    public ICollection<InvoiceTypeAttr> InvoiceTypeAttrs { get; set; }
    public ICollection<Invoice> Invoices { get; set; }
    public ICollection<PartyPrefDocTypeTpl> PartyPrefDocTypeTpls { get; set; }
}