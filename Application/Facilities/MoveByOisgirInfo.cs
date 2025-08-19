using Domain;

namespace Application.Facilities;

public class MoveByOisgirInfo
{
    public Product Product { get; set; }
    public FacilityLocation FacilityLocationFrom { get; set; }
    public FacilityLocation FacilityLocationTo { get; set; }
    public ProductFacilityLocation TargetProductFacilityLocation { get; set; }
    public decimal QuantityOnHandTotalFrom { get; set; }
    public decimal AvailableToPromiseTotalFrom { get; set; }
    public decimal QuantityOnHandTotalTo { get; set; }
    public decimal AvailableToPromiseTotalTo { get; set; }
    public decimal TotalQuantity { get; set; }
    // Add additional fields or nested references if needed
}
