namespace Domain;

public class FacilityAssocType
{
    public FacilityAssocType()
    {
        ProductFacilityAssocs = new HashSet<ProductFacilityAssoc>();
    }

    public string FacilityAssocTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductFacilityAssoc> ProductFacilityAssocs { get; set; }
}