namespace Application.Facilities;

public class CompletePackingResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public string ShipmentId { get; set; }

    public static CompletePackingResult Success(string shipmentId) => new()
    {
        IsSuccess = true,
        ShipmentId = shipmentId
    };

    public static CompletePackingResult Empty() => new()
    {
        IsSuccess = false,
        ShipmentId = "EMPTY",
        ErrorMessage = "No lines to pack."
    };

    public static CompletePackingResult Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}