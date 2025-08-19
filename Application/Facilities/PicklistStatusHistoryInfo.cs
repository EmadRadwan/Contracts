using Domain;

namespace Application.Facilities;

public class PicklistStatusHistoryInfo
{
    public PicklistStatusHistory PicklistStatusHistory { get; set; }
    public StatusItem StatusItem { get; set; }
    public StatusItem StatusItemTo { get; set; }
}