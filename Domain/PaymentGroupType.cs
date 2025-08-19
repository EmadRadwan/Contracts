using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PaymentGroupType
{
    public PaymentGroupType()
    {
        InverseParentType = new HashSet<PaymentGroupType>();
        PaymentGroups = new HashSet<PaymentGroup>();
    }

    public string PaymentGroupTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGroupType? ParentType { get; set; }
    public ICollection<PaymentGroupType> InverseParentType { get; set; }
    public ICollection<PaymentGroup> PaymentGroups { get; set; }
}