using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TaxAuthorityCategory
{
    public string TaxAuthGeoId { get; set; } = null!;
    public string TaxAuthPartyId { get; set; } = null!;
    public string ProductCategoryId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory ProductCategory { get; set; } = null!;
    public TaxAuthority TaxAuth { get; set; } = null!;
}