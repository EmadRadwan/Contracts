namespace Domain;

public class ProductAverageCost
{
    public string ProductAverageCostTypeId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string FacilityId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? AverageCost { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public Party OrganizationParty { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductAverageCostType ProductAverageCostType { get; set; } = null!;
}