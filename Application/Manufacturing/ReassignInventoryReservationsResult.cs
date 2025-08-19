namespace Application.Manufacturing;

public class ReassignInventoryReservationsResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public HashSet<string> NoLongerOnBackOrderIdSet { get; set; }
}