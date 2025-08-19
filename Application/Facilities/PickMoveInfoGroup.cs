using Domain;

namespace Application.Facilities;

public class PickMoveInfoGroup {
    public ShipmentMethodType ShipmentMethodType { get; set; }
    /*public List<OrderHeaderInfo> OrderReadyToPickInfoList { get; set; } = new List<OrderHeaderInfo>();
    public List<OrderHeaderInfo> OrderNeedsStockMoveInfoList { get; set; } = new List<OrderHeaderInfo>();*/
   
    public List<string> OrderReadyToPickInfoList { get; set; } = new List<string>();
    public List<string> OrderNeedsStockMoveInfoList { get; set; } = new List<string>();

    public string GroupName { get; set; }
    public string GroupName1 { get; set; }
    public string GroupName2 { get; set; }
    public string GroupName3 { get; set; }
}
