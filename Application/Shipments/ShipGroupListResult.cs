using Domain;

namespace Application.Shipments;

public class ShipGroupListResult
{
    public bool IsSuccess { get; set; }
    public List<OrderItemShipGroup> ShipGroupList { get; set; }
    public string Message { get; set; }

    public static ShipGroupListResult Success(List<OrderItemShipGroup> shipGroupList) =>
        new ShipGroupListResult { IsSuccess = true, ShipGroupList = shipGroupList };

    public static ShipGroupListResult Failure(string message) =>
        new ShipGroupListResult { IsSuccess = false, Message = message };
}