namespace Application.Facilities.Facilities;

public class FacilityDto
{
    public string? FacilityId { get; set; }
    public string? FacilityTypeId { get; set; }
    public string? FacilityTypeDescription { get; set; }
    public string? FacilityName { get; set; }
    public string? Description { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime CreatedStamp { get; set; }
}