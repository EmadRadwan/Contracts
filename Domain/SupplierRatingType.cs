namespace Domain;

public class SupplierRatingType
{
    public SupplierRatingType()
    {
        SupplierProducts = new HashSet<SupplierProduct>();
    }

    public string SupplierRatingTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<SupplierProduct> SupplierProducts { get; set; }
}