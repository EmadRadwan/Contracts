namespace Domain;

public class ProductFacilityLocation
{
    public string ProductId { get; set; } = null!;
    public string FacilityId { get; set; } = null!;
    public string LocationSeqId { get; set; } = null!;
    public decimal? MinimumStock { get; set; }
    public decimal? MoveQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FacilityLocation FacilityLocation { get; set; } = null!;
    public Product Product { get; set; } = null!;
}