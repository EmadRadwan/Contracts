namespace Domain;

public class ProductGeo
{
    public string ProductId { get; set; } = null!;
    public string GeoId { get; set; } = null!;
    public string? ProductGeoEnumId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Geo Geo { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Enumeration? ProductGeoEnum { get; set; }
}