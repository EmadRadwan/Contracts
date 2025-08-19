using Domain;

namespace Application.Facilities;

public class PicklistInfo
{
    public Picklist Picklist { get; set; }
    public Facility Facility { get; set; }
    public ShipmentMethodType ShipmentMethodType { get; set; }
    public StatusItem StatusItem { get; set; }
    public List<StatusValidChangeToDetailDto> StatusValidChangeToDetailList { get; set; } = new();

    public List<PicklistRoleInfo> PicklistRoleInfoList { get; set; } = new();
    public List<PicklistStatusHistoryInfo> PicklistStatusHistoryInfoList { get; set; } = new();
    public List<PicklistBinInfo> PicklistBinInfoList { get; set; } = new();
}
