namespace Application.Services;

public class VehicleLovDto
{
    public string VehicleId { get; set; }
    public string ChassisNumber { get; set; }
    public string PlateNumber { get; set; }
    public VehiclePartyDto FromPartyId { get; set; }
    public string FromPartyName { get; set; }
    public string MakeDescription { get; set; }
    public string ModelDescription { get; set; }
    public DateTime? ServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
}