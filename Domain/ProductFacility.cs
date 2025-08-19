using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductFacility
{
    public string ProductId { get; set; } = null!;
    public string FacilityId { get; set; } = null!;
    public decimal? MinimumStock { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public int? DaysToShip { get; set; }
    public string? ReplenishMethodEnumId { get; set; }
    public decimal? LastInventoryCount { get; set; }
    public string? RequirementMethodEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Enumeration? ReplenishMethodEnum { get; set; }
    public Enumeration? RequirementMethodEnum { get; set; }
}