using Domain;

namespace Application.Order.Orders.Returns;

public class ReturnItemsDto
{
    public string ReturnId { get; set; }
    public string ReturnItemSeqId { get; set; }
    public string OrderId { get; set; }
    public string StatusId { get; set; }
    public ReturnHeader ReturnHeader { get; set; }
    public string ToPartyId { get; set; }
    public List<ReturnItem> ReturnItems { get; set; }
    public List<ReturnAdjustment> ReturnAdjustments { get; set; }
    public List<ReturnType> ReturnTypes { get; set; }
    public List<StatusItem> ItemStatus { get; set; }
    public List<ReturnReason> ReturnReasons { get; set; }
    public List<StatusItem> ItemStts { get; set; }
    public Dictionary<string, string> ReturnItemTypeMap { get; set; }
    public List<OrderHeaderItemAndRolesDto> PartyOrders { get; set; }
    public string PartyId { get; set; }
    public List<ReturnItemShipment> ReturnItemShipments { get; set; }
    public decimal ShippingAmount { get; set; }
    public OrderReadHelper Orh { get; set; }
    public List<OrderAdjustment> OrderHeaderAdjustments { get; set; }
    public List<ReturnableItemInfo> ReturnableItems { get; set; }
}
