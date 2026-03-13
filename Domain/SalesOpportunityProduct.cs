using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SalesOpportunityProduct
{
    public string SalesOpportunityProductId { get; set; } = null!;
    public string SalesOpportunityId { get; set; } = null!;
    public string? ProductId { get; set; }          // ← apartment / unit (nullable)
    public string? WorkEffortId { get; set; }       // ← project / compound (nullable)

    // Common fields for both cases
    public decimal? Quantity { get; set; } = 1m;    // usually 1 for apartments, can be >1 for bulk/investor
    public string? Notes { get; set; }              // e.g. "Client prefers 3rd floor", "Negotiating 5% discount"
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }         // allows soft-delete / history of interest

    // Audit fields (consistent with your other entities)
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }

    public virtual SalesOpportunity SalesOpportunity { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual WorkEffort? WorkEffort { get; set; }
    
}