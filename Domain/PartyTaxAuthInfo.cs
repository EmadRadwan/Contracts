using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyTaxAuthInfo
{
    public string PartyId { get; set; } = null!;
    public string TaxAuthGeoId { get; set; } = null!;
    public string TaxAuthPartyId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? PartyTaxId { get; set; }
    public string? IsExempt { get; set; }
    public string? IsNexus { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public TaxAuthority TaxAuth { get; set; } = null!;
}