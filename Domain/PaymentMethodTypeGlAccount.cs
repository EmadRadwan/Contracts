using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PaymentMethodTypeGlAccount
{
    public string PaymentMethodTypeId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? GlAccount { get; set; }
    public Party OrganizationParty { get; set; } = null!;
    public PaymentMethodType PaymentMethodType { get; set; } = null!;
}