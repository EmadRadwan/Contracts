using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class AgreementTerm
{
    public AgreementTerm()
    {
        AgreementTermAttributes = new HashSet<AgreementTermAttribute>();
    }

    public string AgreementTermId { get; set; } = null!;
    public string? TermTypeId { get; set; }
    public string? AgreementId { get; set; }
    public string? AgreementItemSeqId { get; set; }
    public string? InvoiceItemTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public double? MinQuantity { get; set; }
    public double? MaxQuantity { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Agreement? Agreement { get; set; }
    public AgreementItem? AgreementI { get; set; }
    public InvoiceItemType? InvoiceItemType { get; set; }
    public TermType? TermType { get; set; }
    public ICollection<AgreementTermAttribute> AgreementTermAttributes { get; set; }
}