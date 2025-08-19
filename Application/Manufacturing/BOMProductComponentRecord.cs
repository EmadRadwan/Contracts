using System.ComponentModel.DataAnnotations;

namespace Application.Manufacturing;

public class BOMProductComponentRecord
{
    [Key] public string ProductId { get; set; }

    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public string ProductIdTo { get; set; }
    public string ProductNameTo { get; set; }
    public string ProductDescriptionTo { get; set; }
    public string ProductAssocTypeId { get; set; }
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
    public string? QuantityUOMId { get; set; }
    public string? QuantityUOMDescription { get; set; }
}