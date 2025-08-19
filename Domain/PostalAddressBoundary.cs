namespace Domain;

public class PostalAddressBoundary
{
    public string ContactMechId { get; set; } = null!;
    public string GeoId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PostalAddress ContactMech { get; set; } = null!;
    public Geo Geo { get; set; } = null!;
}