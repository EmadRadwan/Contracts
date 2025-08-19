using Domain;

namespace Application.Order.Orders;

public class BillingAccountBalanceResult
{
    public decimal AccountBalance { get; set; }
    public decimal NetAccountBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal AvailableToCapture { get; set; }
    public BillingAccount BillingAccount { get; set; } // Assuming BillingAccount is a class representing the entity
}
