namespace Application.WorkEfforts;

public class WorkEffortGoodStandardDto
{
    public string WorkEffortId { get; set; } 
    public string ProductId { get; set; } 
    public string? ProductName { get; set; } 
    public string? ProductQuantityUom { get; set; } 
    public string WorkEffortGoodStdTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public double? EstimatedQuantity { get; set; }
    public decimal? EstimatedCost { get; set; }
    
    public double? IssuedQuantity { get; set; }
    public double? ReturnedQuantity { get; set; }
    
    // New fields for location and lot
    public string? LocationSeqId { get; set; }
    public string? SecondaryLocationSeqId { get; set; }
    public string? LotId { get; set; }
}
