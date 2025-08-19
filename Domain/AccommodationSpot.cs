namespace Domain;

public class AccommodationSpot
{
    public AccommodationSpot()
    {
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string AccommodationSpotId { get; set; } = null!;
    public string? AccommodationClassId { get; set; }
    public string? FixedAssetId { get; set; }
    public int? NumberOfSpaces { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AccommodationClass? AccommodationClass { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}