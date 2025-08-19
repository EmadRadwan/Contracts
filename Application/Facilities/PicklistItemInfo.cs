using Domain;

namespace Application.Facilities;

public class PicklistItemInfo
{
    public PicklistItem PicklistItem { get; set; }
    public PicklistBin PicklistBin { get; set; }
    public OrderItem OrderItem { get; set; }
    public Product Product { get; set; }
    public InventoryItemAndLocationDto InventoryItemAndLocation { get; set; }
    public OrderItemShipGrpInvRes OrderItemShipGrpInvRes { get; set; }
    public List<ItemIssuance> ItemIssuanceList { get; set; } = new();
}
