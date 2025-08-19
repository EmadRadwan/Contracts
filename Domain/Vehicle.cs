namespace Domain;

public class Vehicle
{
    public string VehicleId { get; set; }
    public string ChassisNumber { get; set; }
    public string? Vin { get; set; }
    public int? Year { get; set; }
    public string PlateNumber { get; set; }

    // Rest of your properties

    public string? FromPartyId { get; set; }
    public string MakeId { get; set; }
    public string ModelId { get; set; }
    public string? VehicleTypeId { get; set; }
    public string? TransmissionTypeId { get; set; }
    public string? ExteriorColorId { get; set; }
    public string? InteriorColorId { get; set; }

    public DateTime? ServiceDate { get; set; }
    public int Mileage { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }

    public virtual Party OwnerParty { get; set; }
    public virtual ProductCategory Make { get; set; }
    public virtual ProductCategory Model { get; set; }
    public virtual ProductCategory VehicleType { get; set; }
    public virtual ProductCategory TransmissionType { get; set; }
    public virtual ProductCategory ExteriorColor { get; set; }
    public virtual ProductCategory InteriorColor { get; set; }
    public ICollection<VehicleAnnotation> VehicleAnnotations { get; set; } = new List<VehicleAnnotation>();
    public ICollection<OrderHeader> Orders { get; set; }
    public ICollection<Quote> Quotes { get; set; }
    public ICollection<VehicleContent> VehicleContents { get; set; }
}