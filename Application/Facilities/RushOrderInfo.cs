namespace Application.Facilities;

public class RushOrderInfo {
    public List<OrderHeaderInfo> OrderReadyToPickInfoList { get; set; } = new List<OrderHeaderInfo>();
    public List<OrderHeaderInfo> OrderNeedsStockMoveInfoList { get; set; } = new List<OrderHeaderInfo>();
}
