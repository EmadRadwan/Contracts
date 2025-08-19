namespace Domain;

public class Delivery
{
    public Delivery()
    {
        ShipmentRouteSegments = new HashSet<ShipmentRouteSegment>();
    }

    public string DeliveryId { get; set; } = null!;
    public string? OriginFacilityId { get; set; }
    public string? DestFacilityId { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualArrivalDate { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string? FixedAssetId { get; set; }
    public decimal? StartMileage { get; set; }
    public decimal? EndMileage { get; set; }
    public decimal? FuelUsed { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility? DestFacility { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public Facility? OriginFacility { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegments { get; set; }
}