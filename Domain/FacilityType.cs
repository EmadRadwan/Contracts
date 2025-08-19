using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class FacilityType
{
    public FacilityType()
    {
        Facilities = new HashSet<Facility>();
        FacilityTypeAttrs = new HashSet<FacilityTypeAttr>();
        InverseParentType = new HashSet<FacilityType>();
    }

    public string FacilityTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FacilityType? ParentType { get; set; }
    public ICollection<Facility> Facilities { get; set; }
    public ICollection<FacilityTypeAttr> FacilityTypeAttrs { get; set; }
    public ICollection<FacilityType> InverseParentType { get; set; }
}