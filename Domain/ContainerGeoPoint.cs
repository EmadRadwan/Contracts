namespace Domain;

public class ContainerGeoPoint
{
    public string ContainerId { get; set; } = null!;
    public string GeoPointId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Container Container { get; set; } = null!;
    public GeoPoint GeoPoint { get; set; } = null!;
}