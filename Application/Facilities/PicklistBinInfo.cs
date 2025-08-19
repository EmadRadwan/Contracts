using Domain;

namespace Application.Facilities;

public class PicklistBinInfo
{
    public PicklistBin PicklistBin { get; set; }
    public OrderHeader PrimaryOrderHeader { get; set; }
    public OrderItemShipGroup PrimaryOrderItemShipGroup { get; set; }
    public ProductStore ProductStore { get; set; }
    public List<PicklistItemInfo> PicklistItemInfoList { get; set; } = new();
}