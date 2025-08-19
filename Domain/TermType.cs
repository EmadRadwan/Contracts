using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TermType
{
    public TermType()
    {
        AgreementTerms = new HashSet<AgreementTerm>();
        BillingAccountTerms = new HashSet<BillingAccountTerm>();
        InverseParentType = new HashSet<TermType>();
        InvoiceTerms = new HashSet<InvoiceTerm>();
        OrderTerms = new HashSet<OrderTerm>();
        QuoteTerms = new HashSet<QuoteTerm>();
        TermTypeAttrs = new HashSet<TermTypeAttr>();
    }

    public string TermTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TermType? ParentType { get; set; }
    public ICollection<AgreementTerm> AgreementTerms { get; set; }
    public ICollection<BillingAccountTerm> BillingAccountTerms { get; set; }
    public ICollection<TermType> InverseParentType { get; set; }
    public ICollection<InvoiceTerm> InvoiceTerms { get; set; }
    public ICollection<OrderTerm> OrderTerms { get; set; }
    public ICollection<QuoteTerm> QuoteTerms { get; set; }
    public ICollection<TermTypeAttr> TermTypeAttrs { get; set; }
}