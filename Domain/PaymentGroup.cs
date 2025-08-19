using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PaymentGroup
{
    public PaymentGroup()
    {
        PaymentGroupMembers = new HashSet<PaymentGroupMember>();
    }

    public string PaymentGroupId { get; set; } = null!;
    public string? PaymentGroupTypeId { get; set; }
    public string? PaymentGroupName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PaymentGroupType? PaymentGroupType { get; set; }
    public ICollection<PaymentGroupMember> PaymentGroupMembers { get; set; }
}