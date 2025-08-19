using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class AgreementItem
{
    public AgreementItem()
    {
        Addenda = new HashSet<Addendum>();
        AgreementEmploymentAppls = new HashSet<AgreementEmploymentAppl>();
        AgreementFacilityAppls = new HashSet<AgreementFacilityAppl>();
        AgreementGeographicalApplics = new HashSet<AgreementGeographicalApplic>();
        AgreementItemAttributes = new HashSet<AgreementItemAttribute>();
        AgreementProductAppls = new HashSet<AgreementProductAppl>();
        AgreementPromoAppls = new HashSet<AgreementPromoAppl>();
        AgreementTerms = new HashSet<AgreementTerm>();
        SupplierProducts = new HashSet<SupplierProduct>();
    }

    public string AgreementId { get; set; } = null!;
    public string AgreementItemSeqId { get; set; } = null!;
    public string? AgreementItemTypeId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? AgreementText { get; set; }
    public byte[]? AgreementImage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Agreement Agreement { get; set; } = null!;
    public AgreementItemType? AgreementItemType { get; set; }
    public ICollection<Addendum> Addenda { get; set; }
    public ICollection<AgreementEmploymentAppl> AgreementEmploymentAppls { get; set; }
    public ICollection<AgreementFacilityAppl> AgreementFacilityAppls { get; set; }
    public ICollection<AgreementGeographicalApplic> AgreementGeographicalApplics { get; set; }
    public ICollection<AgreementItemAttribute> AgreementItemAttributes { get; set; }
    public ICollection<AgreementProductAppl> AgreementProductAppls { get; set; }
    public ICollection<AgreementPromoAppl> AgreementPromoAppls { get; set; }
    public ICollection<AgreementTerm> AgreementTerms { get; set; }
    public ICollection<SupplierProduct> SupplierProducts { get; set; }
}