namespace Domain;

public class AccommodationMapType
{
    public AccommodationMapType()
    {
        AccommodationMaps = new HashSet<AccommodationMap>();
    }

    public string AccommodationMapTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<AccommodationMap> AccommodationMaps { get; set; }
}