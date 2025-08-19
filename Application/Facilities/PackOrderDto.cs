namespace Application.Facilities;

public class PackOrderDto
{
    public string OrderId { get; set; }
    public string? ShipGroupSeqId { get; set; }
    public string FacilityId { get; set; }
    public string? PicklistBinId { get; set; }
    public List<PackOrderItemDto> ItemsToPack { get; set; }
}