using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FinAccountStatus
{
    public string FinAccountId { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public DateTime StatusDate { get; set; }
    public DateTime? StatusEndDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public FinAccount FinAccount { get; set; } = null!;
    public StatusItem Status { get; set; } = null!;
}