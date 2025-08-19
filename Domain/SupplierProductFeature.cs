namespace Domain;

public class SupplierProductFeature
{
    public string PartyId { get; set; } = null!;
    public string ProductFeatureId { get; set; } = null!;
    public string? Description { get; set; }
    public string? UomId { get; set; }
    public string? IdCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public ProductFeature ProductFeature { get; set; } = null!;
    public Uom? Uom { get; set; }
}