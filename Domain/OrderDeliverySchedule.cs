namespace Domain;

public class OrderDeliverySchedule
{
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public DateTime? EstimatedReadyDate { get; set; }
    public int? Cartons { get; set; }
    public int? SkidsPallets { get; set; }
    public decimal? UnitsPieces { get; set; }
    public decimal? TotalCubicSize { get; set; }
    public string? TotalCubicUomId { get; set; }
    public decimal? TotalWeight { get; set; }
    public string? TotalWeightUomId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public StatusItem? Status { get; set; }
    public Uom? TotalCubicUom { get; set; }
    public Uom? TotalWeightUom { get; set; }
}