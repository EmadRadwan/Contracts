namespace Domain;

public class FixedAssetGeoPoint
{
    public string FixedAssetId { get; set; } = null!;
    public string GeoPointId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAsset FixedAsset { get; set; } = null!;
    public GeoPoint GeoPoint { get; set; } = null!;
}