namespace Application.Facilities;

public class FindStockMovesNeededResult
{
    public List<MoveByOisgirInfo> MoveByOisgirInfoList { get; set; } = new();
    public Dictionary<string, string> StockMoveHandled { get; set; } = new();
    public List<string> WarningMessageList { get; set; } = new();
}