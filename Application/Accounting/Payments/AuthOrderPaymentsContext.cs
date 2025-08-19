using Domain;

namespace Application.Accounting.Payments;


public class AuthOrderPaymentsContext
{
    public string OrderId { get; set; }
    public UserLogin UserLogin { get; set; }
}

