namespace Application.Order.Orders;

public class BillingAccountModel
{
    public string BillingAccountId { get; set; }
    public string AccountCurrencyUomId { get; set; }
    public decimal AccountBalance { get; set; }
    public decimal AccountLimit { get; set; }
    public decimal AccountAvailable { get; set; }
    public string Description { get; set; }
}
