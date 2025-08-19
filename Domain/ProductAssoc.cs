using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductAssoc
{
    public string ProductId { get; set; } = null!;
    public string ProductIdTo { get; set; } = null!;
    public string ProductAssocTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public string? Reason { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? ScrapFactor { get; set; }
    public string? Instruction { get; set; }
    public string? RoutingWorkEffortId { get; set; }
    public string? EstimateCalcMethod { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? EstimateCalcMethodNavigation { get; set; }
    public Product Product { get; set; } = null!;
    public ProductAssocType ProductAssocType { get; set; } = null!;
    public Product ProductIdToNavigation { get; set; } = null!;
    public RecurrenceInfo? RecurrenceInfo { get; set; }
    public WorkEffort? RoutingWorkEffort { get; set; }
}