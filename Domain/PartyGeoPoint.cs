namespace Domain;

public class PartyGeoPoint
{
    public string PartyId { get; set; } = null!;
    public string GeoPointId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GeoPoint GeoPoint { get; set; } = null!;
    public Party Party { get; set; } = null!;
}