namespace Application.Facilities;

public class LoadPackingDataInput
{
    // Mandatory
    public string FacilityId { get; set; }

    // Optional
    public string ShipmentId { get; set; }
    public string OrderId { get; set; }
    public string ShipGroupSeqId { get; set; }
    public string PicklistBinId { get; set; }
}