namespace Application.Order.Orders;

public class CheckItemStatusOutput
{
    public string errorMessage { get; set; }
    public List<string> errorMessageList { get; set; }
    public string responseMessage { get; set; }
    public string successMessage { get; set; }
    public List<string> successMessageList { get; set; }
}