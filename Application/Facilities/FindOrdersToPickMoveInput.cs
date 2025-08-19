using Domain;

namespace Application.Facilities;

public class FindOrdersToPickMoveInput
{
    public string FacilityId { get; set; } // Required
    public string? ShipmentMethodTypeId { get; set; } // Optional
    public string? IsRushOrder { get; set; } // Optional
    public long? MaxNumberOfOrders { get; set; } // Optional
    public List<string>? OrderIdList { get; set; } // Optional: List of Order IDs
    public List<OrderHeader>? OrderHeaderList { get; set; } // Optional: Preloaded list of OrderHeaders
}
