namespace Application.Accounting.BillingAccounts;

public class OrderPaymentPrefDto
{
public string BillingAccountId { get; set; }
public string OrderId { get; set; }
public DateTime OrderDate { get; set; }
public string PaymentMethodTypeId { get; set; }
public string PaymentStatusId { get; set; }
public string PaymentStatusDescription { get; set; }
public decimal MaxAmount { get; set; }
// Add other necessary fields as needed
    
}