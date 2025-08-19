using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentGlAccountTypeMap
{
    public string PaymentTypeId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccountType? GlAccountType { get; set; }
    public Party OrganizationParty { get; set; } = null!;
    public PaymentType PaymentType { get; set; } = null!;
}