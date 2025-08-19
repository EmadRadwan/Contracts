namespace Application.Facilities;

public class PickMoveInfoGroupDto
{
    public string GroupName { get; set; }
    public string GroupName1 { get; set; }
    public string GroupName2 { get; set; }
    public string GroupName3 { get; set; }

    // Minimal set: just storing orderIds rather than full OrderHeaderInfo
    public List<string> OrderReadyToPickInfoList { get; set; } = new();
    public List<string> OrderNeedsStockMoveInfoList { get; set; } = new();

    // If you need the shipment method name (or ID) on the UI
    public string ShipmentMethodTypeId { get; set; }
}
