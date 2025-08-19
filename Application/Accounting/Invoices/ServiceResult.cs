namespace Application.Shipments.Invoices;

public class ServiceResult
{
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public object Data { get; set; }
}
