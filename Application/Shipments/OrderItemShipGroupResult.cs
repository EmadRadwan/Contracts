using Domain;

namespace Application.Shipments;

public class OrderItemShipGroupResult
{
    // List of approved OrderItem and ShipGroup associations
    public List<OrderItemShipGroup> OrderItemAndShipGroupAssocList { get; }

    // Dictionary mapping shipGroupSeqId to a list of OrderItemShipGroup associations
    public Dictionary<string, List<OrderItemShipGroup>> OrderItemListByShGrpMap { get; }

    // Constructor to initialize the properties
    public OrderItemShipGroupResult(List<OrderItemShipGroup> orderItemAndShipGroupAssocList,
        Dictionary<string, List<OrderItemShipGroup>> orderItemListByShGrpMap)
    {
        OrderItemAndShipGroupAssocList = orderItemAndShipGroupAssocList;
        OrderItemListByShGrpMap = orderItemListByShGrpMap;
    }
}

