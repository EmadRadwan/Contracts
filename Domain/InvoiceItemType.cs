using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InvoiceItemType
{
    public InvoiceItemType()
    {
        AgreementTerms = new HashSet<AgreementTerm>();
        InverseParentType = new HashSet<InvoiceItemType>();
        InvoiceItemTypeAttrs = new HashSet<InvoiceItemTypeAttr>();
        InvoiceItemTypeGlAccounts = new HashSet<InvoiceItemTypeGlAccount>();
        InvoiceItemTypeMaps = new HashSet<InvoiceItemTypeMap>();
        InvoiceItems = new HashSet<InvoiceItem>();
    }

    public string InvoiceItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }

    public string? DefaultGlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? DefaultGlAccount { get; set; }
    public InvoiceItemType? ParentType { get; set; }
    public ICollection<AgreementTerm> AgreementTerms { get; set; }
    public ICollection<InvoiceItemType> InverseParentType { get; set; }
    public ICollection<InvoiceItemTypeAttr> InvoiceItemTypeAttrs { get; set; }
    public ICollection<InvoiceItemTypeGlAccount> InvoiceItemTypeGlAccounts { get; set; }
    public ICollection<InvoiceItemTypeMap> InvoiceItemTypeMaps { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
}