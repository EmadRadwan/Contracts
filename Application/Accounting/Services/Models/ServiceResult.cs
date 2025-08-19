namespace Application.Accounting.Services.Models;

// Generic service result DTO that encapsulates success or error information.
public class ServiceResult<T>
{
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public T Data { get; set; }
}



