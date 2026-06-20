using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProjectCommissionRate
{
    public string ProjectCommissionRateId { get; set; } = null!;

    public string ProjectId { get; set; } = null!;

    // COMM_SALE_DIRECT | COMM_SALE_PERSONAL | COMM_SALE_INDIRECT
    public string SaleTypeId { get; set; } = null!;

    // Internal sales rep — used by all three sale types
    public decimal SalesRepPercent { get; set; }

    // Internal manager — used by all three sale types
    public decimal ManagerPercent { get; set; }

    // INDIRECT only: total commission paid to the external broker company
    public decimal? ExternalCompanyPercent { get; set; }

    // INDIRECT only: portion tracked for the broker's sales rep
    public decimal? ExternalSalesRepPercent { get; set; }

    // INDIRECT only: portion tracked for the broker's manager
    public decimal? ExternalManagerPercent { get; set; }

    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    public virtual WorkEffort Project { get; set; } = null!;
}