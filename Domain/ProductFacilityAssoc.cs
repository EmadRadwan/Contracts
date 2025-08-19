namespace Domain;

public class ProductFacilityAssoc
{
    public string ProductId { get; set; } = null!;
    public string FacilityId { get; set; } = null!;
    public string FacilityIdTo { get; set; } = null!;
    public string FacilityAssocTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public int? SequenceNum { get; set; }
    public int? TransitTime { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Facility Facility { get; set; } = null!;
    public FacilityAssocType FacilityAssocType { get; set; } = null!;
    public Facility FacilityIdToNavigation { get; set; } = null!;
    public Product Product { get; set; } = null!;
}