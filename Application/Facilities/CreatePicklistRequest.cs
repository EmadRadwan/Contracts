namespace Application.Facilities;

public class CreatePicklistRequest
{
    public string FacilityId { get; set; }
    public List<string> OrderReadyToPickInfoList { get; set; } = new();
    public List<string> OrderNeedsStockMoveInfoList { get; set; } = new();
}
