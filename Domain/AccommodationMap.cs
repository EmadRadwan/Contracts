namespace Domain;

public class AccommodationMap
{
    public AccommodationMap()
    {
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string AccommodationMapId { get; set; } = null!;
    public string? AccommodationClassId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? AccommodationMapTypeId { get; set; }
    public int? NumberOfSpaces { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AccommodationClass? AccommodationClass { get; set; }
    public AccommodationMapType? AccommodationMapType { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}