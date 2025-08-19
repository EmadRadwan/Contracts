namespace Domain;

public class GeoAssoc
{
    public string GeoId { get; set; } = null!;
    public string GeoIdTo { get; set; } = null!;
    public string? GeoAssocTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo Geo { get; set; } = null!;
    public GeoAssocType? GeoAssocType { get; set; }
    public Geo GeoIdToNavigation { get; set; } = null!;
}