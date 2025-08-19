using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentType
{
    public PaymentType()
    {
        InverseParentType = new HashSet<PaymentType>();
        PaymentGlAccountTypeMaps = new HashSet<PaymentGlAccountTypeMap>();
        PaymentTypeAttrs = new HashSet<PaymentTypeAttr>();
        Payments = new HashSet<Payment>();
    }

    public string PaymentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentType? ParentType { get; set; }
    public ICollection<PaymentType> InverseParentType { get; set; }
    public ICollection<PaymentGlAccountTypeMap> PaymentGlAccountTypeMaps { get; set; }
    public ICollection<PaymentTypeAttr> PaymentTypeAttrs { get; set; }
    public ICollection<Payment> Payments { get; set; }
}