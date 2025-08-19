namespace Application.Shipments.Costs;

public class FixedAssetStdCostsDto
{
    public string FixedAssetId { get; set; }
    public string FixedAssetStdCostId { get; set; }
    public string FixedAssetStdCostTypeId { get; set; }
    public string AmountUomId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal Amount { get; set; }
}