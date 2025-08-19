using Application.Core;

namespace Application.Services;

public class VehicleParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? ChassisNumber { get; set; }
    public string? Vin { get; set; }
    public string? PlateNumber { get; set; }
    public string? OwnerPartyId { get; set; }
    public string? MakeId { get; set; }
    public string? ModelId { get; set; }
    public string? VehicleTypeId { get; set; }
}