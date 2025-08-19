namespace Domain;

public class ReorderGuideline
{
    public string ReorderGuidelineId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? PartyId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? FacilityId { get; set; }
    public string? GeoId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public decimal? ReorderLevel { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility? Facility { get; set; }
    public Geo? Geo { get; set; }
    public Party? Party { get; set; }
    public Product? Product { get; set; }
}