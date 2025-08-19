namespace Domain;

public class ShipmentBoxType
{
    public ShipmentBoxType()
    {
        CarrierShipmentBoxTypes = new HashSet<CarrierShipmentBoxType>();
        Products = new HashSet<Product>();
        ShipmentPackages = new HashSet<ShipmentPackage>();
    }

    public string ShipmentBoxTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DimensionUomId { get; set; }
    public decimal? BoxLength { get; set; }
    public decimal? BoxWidth { get; set; }
    public decimal? BoxHeight { get; set; }
    public string? WeightUomId { get; set; }
    public decimal? BoxWeight { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? DimensionUom { get; set; }
    public Uom? WeightUom { get; set; }
    public ICollection<CarrierShipmentBoxType> CarrierShipmentBoxTypes { get; set; }
    public ICollection<Product> Products { get; set; }
    public ICollection<ShipmentPackage> ShipmentPackages { get; set; }
}