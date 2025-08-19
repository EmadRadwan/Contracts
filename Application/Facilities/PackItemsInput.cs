namespace Application.Facilities;

public class PackItemsInput
{
    public string FacilityId { get; set; }
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string PicklistBinId { get; set; }
    public List<PackItemLineDto> ItemsToPack { get; set; }
}