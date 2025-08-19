using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class QuoteType
{
    public QuoteType()
    {
        InverseParentType = new HashSet<QuoteType>();
        PartyPrefDocTypeTpls = new HashSet<PartyPrefDocTypeTpl>();
        QuoteTypeAttrs = new HashSet<QuoteTypeAttr>();
        Quotes = new HashSet<Quote>();
    }

    public string QuoteTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public QuoteType? ParentType { get; set; }
    public ICollection<QuoteType> InverseParentType { get; set; }
    public ICollection<PartyPrefDocTypeTpl> PartyPrefDocTypeTpls { get; set; }
    public ICollection<QuoteTypeAttr> QuoteTypeAttrs { get; set; }
    public ICollection<Quote> Quotes { get; set; }
}