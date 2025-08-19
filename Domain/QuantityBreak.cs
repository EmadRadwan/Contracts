namespace Domain;

public class QuantityBreak
{
    public QuantityBreak()
    {
        ShipmentCostEstimatePriceBreaks = new HashSet<ShipmentCostEstimate>();
        ShipmentCostEstimateQuantityBreaks = new HashSet<ShipmentCostEstimate>();
        ShipmentCostEstimateWeightBreaks = new HashSet<ShipmentCostEstimate>();
    }

    public string QuantityBreakId { get; set; } = null!;
    public string? QuantityBreakTypeId { get; set; }
    public decimal? FromQuantity { get; set; }
    public decimal? ThruQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public QuantityBreakType? QuantityBreakType { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimatePriceBreaks { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateQuantityBreaks { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimateWeightBreaks { get; set; }
}