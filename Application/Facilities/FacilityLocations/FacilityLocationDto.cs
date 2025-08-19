namespace Application.Facilities.FacilityLocations;

public class FacilityLocationDto
{
    public string FacilityId { get; set; } = null!;
    public string LocationSeqId { get; set; } = null!;
    public string? FacilityName { get; set; }
    public string? LocationTypeEnumId { get; set; }
    public string? LocationTypeEnumDescription { get; set; }
    public string? AreaId { get; set; }
    public string? AisleId { get; set; }
    public string? SectionId { get; set; }
    public string? LevelId { get; set; }
    public string? PositionId { get; set; }

}