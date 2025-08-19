using Domain;

namespace Application.Facilities;

public class OrderItemShipGrpInvResInfo {
    public OrderItemShipGrpInvRes OrderItemShipGrpInvRes { get; set; }
    public InventoryItem InventoryItem { get; set; }
    public FacilityLocation FacilityLocation { get; set; }
}