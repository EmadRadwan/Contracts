using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TaxAuthorityRateType
{
    public TaxAuthorityRateType()
    {
        TaxAuthorityRateProducts = new HashSet<TaxAuthorityRateProduct>();
    }

    public string TaxAuthorityRateTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<TaxAuthorityRateProduct> TaxAuthorityRateProducts { get; set; }
}