namespace Application.Facilities;

public class GetPicklistDisplayInfoResult
{
    public int ViewIndex { get; set; }
    public int ViewSize { get; set; }
    public int LowIndex { get; set; }
    public int HighIndex { get; set; }
    public long PicklistCount { get; set; }
    public List<PicklistInfo> PicklistInfoList { get; set; } = new List<PicklistInfo>();
}