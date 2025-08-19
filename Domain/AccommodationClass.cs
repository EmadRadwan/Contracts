namespace Domain;

public class AccommodationClass
{
    public AccommodationClass()
    {
        AccommodationMaps = new HashSet<AccommodationMap>();
        AccommodationSpots = new HashSet<AccommodationSpot>();
        InverseParentClass = new HashSet<AccommodationClass>();
    }

    public string AccommodationClassId { get; set; } = null!;
    public string? ParentClassId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AccommodationClass? ParentClass { get; set; }
    public ICollection<AccommodationMap> AccommodationMaps { get; set; }
    public ICollection<AccommodationSpot> AccommodationSpots { get; set; }
    public ICollection<AccommodationClass> InverseParentClass { get; set; }
}