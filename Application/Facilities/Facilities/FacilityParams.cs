using Application.Core;

namespace Application.Facilities.Facilities;

public class FacilityParams : PaginationParams
{
    public string? FacilityTypeId { get; set; }

    public string? FacilityName { get; set; }
}