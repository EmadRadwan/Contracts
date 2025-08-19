namespace Application.Services;

public class VehicleDto
{
    public string VehicleId { get; set; }
    public string ChassisNumber { get; set; }
    public string Vin { get; set; }
    public int? Year { get; set; }
    public string PlateNumber { get; set; }


    public VehiclePartyDto FromPartyId { get; set; }
    public string FromPartyName { get; set; }
    public string OwnerPartyName { get; set; }
    public string MakeId { get; set; }
    public string MakeDescription { get; set; }
    public string ModelId { get; set; }
    public string ModelDescription { get; set; }
    public string VehicleTypeId { get; set; }
    public string VehicleTypeDescription { get; set; }
    public string TransmissionTypeId { get; set; }
    public string TransmissionTypeDescription { get; set; }
    public string ExteriorColorId { get; set; }
    public string ExteriorColorDescription { get; set; }
    public string InteriorColorId { get; set; }
    public string InteriorColorDescription { get; set; }

    public DateTime? ServiceDate { get; set; }
    public int Mileage { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
}