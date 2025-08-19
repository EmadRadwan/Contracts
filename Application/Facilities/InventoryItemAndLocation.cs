namespace Application.Facilities;

public class InventoryItemAndLocationDto
{
    // from InventoryItem
    public string InventoryItemId { get; set; }
    public string FacilityId { get; set; }
    public string LocationSeqId { get; set; }
    public string ProductId { get; set; }
    public string LotId { get; set; }

    // from FacilityLocation
    public string LocationTypeEnumId { get; set; }
    
}
